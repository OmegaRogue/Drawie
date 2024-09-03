﻿using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class VertexBuffer : BufferObject
{
    public VertexBuffer(Vk vk, Device device, PhysicalDevice physicalDevice, ulong size) : base(vk, device, physicalDevice, size, BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.DeviceLocalBit) 
    {
    }
}