using Drawie.Core;
using Drawie.Core.Bridge;
using Drawie.Core.ColorsImpl;
using Drawie.Core.Surfaces;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.Silk.Extensions;
using PixiEditor.Numerics;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SkiaSharp;

namespace Drawie.Silk;

public class GlfwWindow : Drawie.Windowing.IWindow
{
    private IWindow? window;
    private bool isRunning;

    public string Name
    {
        get => window?.Title ?? string.Empty;
        set
        {
            if (window != null) window.Title = value;
        }
    }

    public VecI Size
    {
        get => window?.Size.ToVecI() ?? VecI.Zero;
        set
        {
            if (window != null) window.Size = value.ToVector2DInt();
        }
    }

    public IWindowRenderApi RenderApi { get; set; }

    public event Action<double> Update;
    public event Action<Texture, double> Render;

    private SKSurface? surface;
    private Texture renderTexture;
    private GRContext context;

    public GlfwWindow(string name, VecI size, IWindowRenderApi renderApi)
    {
        window = Window.Create(WindowOptions.Default with
        {
            Title = name,
            Size = size.ToVector2DInt(),
            API = renderApi.GraphicsApi.ToSilkGraphicsApi()
        });
        
        RenderApi = renderApi;
    }

    public void Show()
    {
        if (!isRunning)
        {
            window.Initialize();

            if (RenderApi.GraphicsApi == GraphicsApi.Vulkan)
            {
                RenderApi.CreateInstance(window.VkSurface, window.Size.ToVecI());
            }
            else
            {
                throw new NotSupportedException($"Provided graphics API '{RenderApi.GraphicsApi}' is not supported.");
            }

            window.FramebufferResize += WindowOnFramebufferResize;
            RenderApi.FramebufferResized += RenderApiOnFramebufferResized;

            CreateRenderTarget(window.FramebufferSize.ToVecI());
            
            window.Render += OnRender;
            window.Render += RenderApi.Render;

            window.Update += OnUpdate;
            isRunning = true;
            window.Run();
        }
    }

    private void RenderApiOnFramebufferResized()
    {
        renderTexture.Dispose();
        renderTexture = null!;
        surface = null!;

        CreateRenderTarget(window!.FramebufferSize.ToVecI(), (VulkanWindowRenderApi)RenderApi);
    }

    private void CreateRenderTarget(VecI size)
    {
        var imageInfo = new GRVkImageInfo()
        {
            CurrentQueueFamily = 0,
            Format = vkRenderApi.texture.ImageFormat,
            Image = vkRenderApi.texture.VkImage.Handle,
            ImageLayout = VulkanTexture.ColorAttachmentOptimal,
            ImageTiling = vkRenderApi.texture.Tiling,
            ImageUsageFlags = vkRenderApi.texture.UsageFlags,
            LevelCount = 1,
            SampleCount = 1,
            Protected = false,
            SharingMode = vkRenderApi.texture.TargetSharingMode
        };

        surface = SKSurface.Create(context, new GRBackendRenderTarget(size.X, size.Y, 1, imageInfo),
            GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));

        renderTexture = Texture.FromExisting(DrawingSurface.FromNative(surface));

        vkRenderApi.texture.TransitionLayoutTo(0, VulkanTexture.ShaderReadOnlyOptimal);
    }

    private void WindowOnFramebufferResize(Vector2D<int> newSize)
    {
        RenderApi.UpdateFramebufferSize(newSize.X, newSize.Y);
    }

    private void OnUpdate(double dt)
    {
        Update?.Invoke(dt);
    }

    private void OnRender(double dt)
    {
        RenderApi.PrepareTextureToWrite();
        renderTexture.DrawingSurface?.Canvas.Clear(Colors.LightCoral);
        Render?.Invoke(renderTexture, dt);
        renderTexture.DrawingSurface?.Flush();
    }

    public void Close()
    {
        window.Update -= OnUpdate;
        window.Render -= OnRender;
        renderTexture.Dispose();
        RenderApi.DestroyInstance();

        window?.Close();
        window?.Dispose();
    }
}