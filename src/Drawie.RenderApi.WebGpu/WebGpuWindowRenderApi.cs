using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.RenderApi.WebGpu.Extensions;
using Evergine.Bindings.WebGPU;
using PixiEditor.Numerics;
using Silk.NET.Core.Contexts;
using static Evergine.Bindings.WebGPU.WebGPUNative;

namespace Drawie.RenderApi.WebGpu;

/*
 * TODO:
 * [] - Add texture support
 * [] - Draw rectangle with texture
 * [] - Convert rectangle into big clipped triangle
 */
public class WebGpuWindowRenderApi : IWindowRenderApi
{
    public GraphicsApi GraphicsApi { get; }

    public event Action? FramebufferResized;

    private WGPUInstance Instance;
    private WGPUSurface Surface;
    private WGPUAdapter Adapter;
    private WGPUAdapterProperties AdapterProperties;
    private WGPUSupportedLimits AdapterLimits;
    private WGPUDevice Device;
    private WGPUTextureFormat SwapChainFormat;
    private WGPUQueue Queue;

    private WGPUPipelineLayout pipelineLayout;
    private WGPURenderPipeline pipeline;
    private WGPUBuffer vertexBuffer;

    private VecI framebufferSize;

    public void CreateInstance(object surface, VecI size)
    {
        if (surface is not INativeWindow nativeWindow)
        {
            throw new ArgumentException("Surface must be a window handle");
        }

        framebufferSize = size;

        InitWebGpu(size, nativeWindow);
        InitResources();
    }

    public unsafe void Render(double deltaTime)
    {
        WGPUSurfaceTexture surfaceTexture = default;
        wgpuSurfaceGetCurrentTexture(Surface, &surfaceTexture);

        if (surfaceTexture.status == WGPUSurfaceGetCurrentTextureStatus.Outdated || surfaceTexture.suboptimal)
        {
            ReconfigureSwapchain();
            return;
        }

        WGPUTextureView nextView = wgpuTextureCreateView(surfaceTexture.texture, null);

        WGPUCommandEncoderDescriptor encoderDescriptor = new WGPUCommandEncoderDescriptor()
        {
            nextInChain = null,
        };
        WGPUCommandEncoder encoder = wgpuDeviceCreateCommandEncoder(Device, &encoderDescriptor);

        WGPURenderPassColorAttachment renderPassColorAttachment = new WGPURenderPassColorAttachment()
        {
            view = nextView,
            resolveTarget = WGPUTextureView.Null,
            loadOp = WGPULoadOp.Clear,
            storeOp = WGPUStoreOp.Store,
            clearValue = new WGPUColor() { a = 1.0f },
        };

        WGPURenderPassDescriptor renderPassDescriptor = new WGPURenderPassDescriptor()
        {
            nextInChain = null,
            colorAttachmentCount = 1,
            colorAttachments = &renderPassColorAttachment,
            depthStencilAttachment = null,
            timestampWrites = null,
        };

        WGPURenderPassEncoder renderPass = wgpuCommandEncoderBeginRenderPass(encoder, &renderPassDescriptor);

        wgpuRenderPassEncoderSetPipeline(renderPass, pipeline);
        wgpuRenderPassEncoderSetVertexBuffer(renderPass, 0, vertexBuffer, 0, WGPU_WHOLE_MAP_SIZE);
        wgpuRenderPassEncoderDraw(renderPass, 3, 1, 0, 0);
        wgpuRenderPassEncoderEnd(renderPass);

        wgpuTextureViewRelease(nextView);

        WGPUCommandBufferDescriptor commandBufferDescriptor = new WGPUCommandBufferDescriptor()
        {
            nextInChain = null,
        };

        WGPUCommandBuffer command = wgpuCommandEncoderFinish(encoder, &commandBufferDescriptor);
        wgpuQueueSubmit(Queue, 1, &command);

        wgpuCommandEncoderRelease(encoder);

        wgpuSurfacePresent(Surface);
    }

    private void ReconfigureSwapchain()
    {
        ConfigureSwapchain(framebufferSize);
        FramebufferResized?.Invoke();
    }

    public void DestroyInstance()
    {
        wgpuSurfaceRelease(Surface);
        wgpuDeviceDestroy(Device);
        wgpuDeviceRelease(Device);
        wgpuAdapterRelease(Adapter);
        wgpuInstanceRelease(Instance);
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        framebufferSize = new VecI(width, height);
    }

    public void PrepareTextureToWrite()
    {
        // Not needed
    }

    private void InitWebGpu(VecI size, INativeWindow nativeWindow)
    {
        CreateInstance();
        CreateSurface(nativeWindow);
        CreateDevice();
        ConfigureSwapchain(size);
    }

    private unsafe void ConfigureSwapchain(VecI size)
    {
        SwapChainFormat = wgpuSurfaceGetPreferredFormat(Surface, Adapter);

        WGPUSurfaceConfiguration surfaceConfiguration = new WGPUSurfaceConfiguration()
        {
            nextInChain = null,
            device = Device,
            format = SwapChainFormat,
            usage = WGPUTextureUsage.RenderAttachment,
            width = (uint)size.X,
            height = (uint)size.Y,
            presentMode = WGPUPresentMode.Fifo,
        };

        wgpuSurfaceConfigure(Surface, &surfaceConfiguration);
    }

    private unsafe void CreateDevice()
    {
        WGPURequestAdapterOptions options = new WGPURequestAdapterOptions()
        {
            nextInChain = null,
            compatibleSurface = Surface,
            powerPreference = WGPUPowerPreference.HighPerformance
        };

        wgpuInstanceRequestAdapter(Instance, &options, OnAdapterRequestEnded, (void*)0);

        WGPUDeviceDescriptor deviceDescriptor = new WGPUDeviceDescriptor()
        {
            nextInChain = null,
            label = null,
            requiredFeatures = (WGPUFeatureName*)0,
            requiredLimits = null,
        };

        wgpuAdapterRequestDevice(Adapter, &deviceDescriptor, OnDeviceRequestEnded, (void*)0);

        wgpuDeviceSetUncapturedErrorCallback(Device, HandleUncapturedErrorCallback, (void*)0);

        Queue = wgpuDeviceGetQueue(Device);
    }

    private unsafe void CreateSurface(INativeWindow nativeWindow)
    {
        var window = nativeWindow.Win32;

        WGPUSurfaceDescriptorFromWindowsHWND windowsSurface = new WGPUSurfaceDescriptorFromWindowsHWND()
        {
            chain = new WGPUChainedStruct()
            {
                sType = WGPUSType.SurfaceDescriptorFromWindowsHWND,
            },
            hinstance = window.Value.HInstance.ToPointer(),
            hwnd = window.Value.Hwnd.ToPointer(),
        };

        WGPUSurfaceDescriptor surfaceDescriptor = new WGPUSurfaceDescriptor()
        {
            nextInChain = &windowsSurface.chain,
        };

        Surface = wgpuInstanceCreateSurface(Instance, &surfaceDescriptor);
    }

    private unsafe void CreateInstance()
    {
        WGPUInstanceExtras instanceExtras = new WGPUInstanceExtras()
        {
            chain = new WGPUChainedStruct()
            {
                sType = (WGPUSType)WGPUNativeSType.InstanceExtras,
            },
            backends = WGPUInstanceBackend.Vulkan,
        };

        WGPUInstanceDescriptor instanceDescriptor = new WGPUInstanceDescriptor()
        {
            nextInChain = &instanceExtras.chain,
        };
        Instance = wgpuCreateInstance(&instanceDescriptor);
    }

    private unsafe void InitResources()
    {
        WGPUPipelineLayoutDescriptor layoutDescription = new()
        {
            nextInChain = null,
            bindGroupLayoutCount = 0,
            bindGroupLayouts = null,
        };

        pipelineLayout = wgpuDeviceCreatePipelineLayout(Device, &layoutDescription);

        string shaderSource = File.ReadAllText(Path.Combine("Shaders", "wgpu_shader.wgsl"));

        WGPUShaderModuleWGSLDescriptor shaderCodeDescriptor = new()
        {
            chain = new WGPUChainedStruct()
            {
                next = null,
                sType = WGPUSType.ShaderModuleWGSLDescriptor,
            },
            code = shaderSource.ToPointer(),
        };

        WGPUShaderModuleDescriptor moduleDescriptor = new()
        {
            nextInChain = &shaderCodeDescriptor.chain,
            hintCount = 0,
            hints = null,
        };

        WGPUShaderModule shaderModule = wgpuDeviceCreateShaderModule(Device, &moduleDescriptor);

        WGPUVertexAttribute* vertexAttributes = stackalloc WGPUVertexAttribute[2]
        {
            new WGPUVertexAttribute()
            {
                format = WGPUVertexFormat.Float32x4,
                offset = 0,
                shaderLocation = 0,
            },
            new WGPUVertexAttribute()
            {
                format = WGPUVertexFormat.Float32x4,
                offset = 16,
                shaderLocation = 1,
            },
        };

        WGPUVertexBufferLayout vertexLayout = new WGPUVertexBufferLayout()
        {
            attributeCount = 2,
            attributes = vertexAttributes,
            arrayStride = (ulong)sizeof(Vector4) * 2,
            stepMode = WGPUVertexStepMode.Vertex,
        };

        WGPUBlendState blendState = new WGPUBlendState()
        {
            color = new WGPUBlendComponent()
            {
                srcFactor = WGPUBlendFactor.One,
                dstFactor = WGPUBlendFactor.Zero,
                operation = WGPUBlendOperation.Add,
            },
            alpha = new WGPUBlendComponent()
            {
                srcFactor = WGPUBlendFactor.One,
                dstFactor = WGPUBlendFactor.Zero,
                operation = WGPUBlendOperation.Add,
            }
        };

        WGPUColorTargetState colorTargetState = new WGPUColorTargetState()
        {
            nextInChain = null,
            format = SwapChainFormat,
            blend = &blendState,
            writeMask = WGPUColorWriteMask.All,
        };

        WGPUFragmentState fragmentState = new WGPUFragmentState()
        {
            nextInChain = null,
            module = shaderModule,
            entryPoint = "fragmentMain".ToPointer(),
            constantCount = 0,
            constants = null,
            targetCount = 1,
            targets = &colorTargetState,
        };

        WGPURenderPipelineDescriptor pipelineDescriptor = new WGPURenderPipelineDescriptor()
        {
            layout = pipelineLayout,
            vertex = new WGPUVertexState()
            {
                bufferCount = 1,
                buffers = &vertexLayout,

                module = shaderModule,
                entryPoint = "vertexMain".ToPointer(),
                constantCount = 0,
                constants = null,
            },
            primitive = new WGPUPrimitiveState()
            {
                topology = WGPUPrimitiveTopology.TriangleList,
                stripIndexFormat = WGPUIndexFormat.Undefined,
                frontFace = WGPUFrontFace.CCW,
                cullMode = WGPUCullMode.None,
            },
            fragment = &fragmentState,
            depthStencil = null,
            multisample = new WGPUMultisampleState()
            {
                count = 1,
                mask = ~0u,
                alphaToCoverageEnabled = false,
            }
        };

        pipeline = wgpuDeviceCreateRenderPipeline(Device, &pipelineDescriptor);

        wgpuShaderModuleRelease(shaderModule);

        // triangle
        Vector4* vertexData = stackalloc Vector4[]
        {
            new Vector4(-1.0f, 3f, 0.5f, 1.0f),
            new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            
            new Vector4(3f, -1f, 0.5f, 1.0f),
            
            new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            
            new Vector4(-1f, -1f, 0.5f, 1.0f),
            
            new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
        };

        ulong size = (ulong)(6 * sizeof(Vector4));
        WGPUBufferDescriptor bufferDescriptor = new WGPUBufferDescriptor()
        {
            nextInChain = null,
            usage = WGPUBufferUsage.Vertex | WGPUBufferUsage.CopyDst,
            size = size,
            mappedAtCreation = false,
        };
        
        vertexBuffer = wgpuDeviceCreateBuffer(Device, &bufferDescriptor);
        wgpuQueueWriteBuffer(Queue, vertexBuffer, 0, vertexData, size);
    }

    private static unsafe void HandleUncapturedErrorCallback(WGPUErrorType type, char* pMessage, void* pUserData)
    {
        Console.WriteLine($"Uncaptured device error: type: {type} ({StringExtensions.GetString(pMessage)})");
    }

    private unsafe void OnAdapterRequestEnded(WGPURequestAdapterStatus status, WGPUAdapter candidateAdapter,
        char* message,
        void* pUserData)
    {
        if (status == WGPURequestAdapterStatus.Success)
        {
            Adapter = candidateAdapter;
            WGPUAdapterProperties properties;
            wgpuAdapterGetProperties(candidateAdapter, &properties);

            WGPUSupportedLimits limits;
            wgpuAdapterGetLimits(candidateAdapter, &limits);

            AdapterProperties = properties;
            AdapterLimits = limits;
        }
        else
        {
            Console.WriteLine($"Could not get WebGPU adapter: {StringExtensions.GetString(message)}");
        }
    }

    private unsafe void OnDeviceRequestEnded(WGPURequestDeviceStatus status, WGPUDevice device, char* message,
        void* pUserData)
    {
        if (status == WGPURequestDeviceStatus.Success)
        {
            Device = device;
        }
        else
        {
            Console.WriteLine($"Could not get WebGPU device: {StringExtensions.GetString(message)}");
        }
    }
}