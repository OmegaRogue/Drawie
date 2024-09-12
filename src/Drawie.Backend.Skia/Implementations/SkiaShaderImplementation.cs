﻿using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using PixiEditor.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaShaderImplementation : SkObjectImplementation<SKShader>, IShaderImplementation
    {
        private Dictionary<IntPtr, SKRuntimeEffect> runtimeEffects = new();
        public SkiaShaderImplementation()
        {
        }

        public IntPtr CreateShader()
        {
            SKShader skShader = SKShader.CreateEmpty();
            ManagedInstances[skShader.Handle] = skShader;
            return skShader.Handle;
        }

        public Shader? CreateFromSksl(string sksl, bool isOpaque, Uniforms uniforms, out string errors)
        {
            SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(sksl, out errors);
            if (string.IsNullOrEmpty(errors))
            {
                SKRuntimeEffectUniforms effectUniforms = UniformsToSkUniforms(uniforms, effect); 
                SKRuntimeEffectChildren effectChildren = UniformsToSkChildren(uniforms, effect);
                SKShader shader = effect.ToShader(effectUniforms, effectChildren);
                ManagedInstances[shader.Handle] = shader;
                runtimeEffects[shader.Handle] = effect;
                return new Shader(shader.Handle);
            }
            
            return null;
        }
        
        public Shader? CreateFromSksl(string sksl, bool isOpaque, out string errors)
        {
            SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(sksl, out errors);
            if (string.IsNullOrEmpty(errors))
            {
                SKShader shader = effect.ToShader();
                ManagedInstances[shader.Handle] = shader;
                return new Shader(shader.Handle);
            }
            
            return null;
        }
        
        public Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors)
        {
            SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint(p1.X, p1.Y), 
                new SKPoint(p2.X, p2.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                null, 
                SKShaderTileMode.Clamp);
            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
        {
            SKShader shader = SKShader.CreatePerlinNoiseTurbulence(
                baseFrequencyX,
                baseFrequencyY,
                numOctaves,
                seed);

            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }
        
        public Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
        {
            if(baseFrequencyX <= 0 || baseFrequencyY <= 0)
                throw new ArgumentException("Base frequency must be greater than 0");
            
            SKShader shader = SKShader.CreatePerlinNoiseFractalNoise(
                baseFrequencyX,
                baseFrequencyY,
                numOctaves,
                seed);

            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public object GetNativeShader(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer]; 
        }

        public Shader WithUpdatedUniforms(IntPtr objectPointer, Uniforms uniforms)
        {
            if (!ManagedInstances.TryGetValue(objectPointer, out var shader))
            {
                 throw new InvalidOperationException("Shader does not exist");
            }
            if (!runtimeEffects.TryGetValue(objectPointer, out var effect))
            {
                throw new InvalidOperationException("Shader is not a runtime effect shader");
            }
            
            // TODO: Don't reupload shaders if they are the same
            SKRuntimeEffectUniforms effectUniforms = UniformsToSkUniforms(uniforms, effect);
            SKRuntimeEffectChildren effectChildren = UniformsToSkChildren(uniforms, effect);
            
            shader.Dispose();
            ManagedInstances.TryRemove(objectPointer, out _);
            runtimeEffects.Remove(objectPointer);
            
            var newShader = effect.ToShader(effectUniforms, effectChildren);
            ManagedInstances[newShader.Handle] = newShader;
            runtimeEffects[newShader.Handle] = effect;
            
            return new Shader(newShader.Handle);
        }

        public void SetLocalMatrix(IntPtr objectPointer, Matrix3X3 matrix)
        {
            if (!ManagedInstances.TryGetValue(objectPointer, out var shader))
            {
                throw new InvalidOperationException("Shader does not exist");
            }
            
            shader.WithLocalMatrix(matrix.ToSkMatrix());
        }

        public void Dispose(IntPtr shaderObjPointer)
        {
            if (!ManagedInstances.TryGetValue(shaderObjPointer, out var shader)) return;
            shader.Dispose();
            ManagedInstances.TryRemove(shaderObjPointer, out _);
        }
        
        private SKRuntimeEffectUniforms UniformsToSkUniforms(Uniforms uniforms, SKRuntimeEffect effect)
        {
            SKRuntimeEffectUniforms skUniforms = new SKRuntimeEffectUniforms(effect);
            foreach (var uniform in uniforms)
            {
                if (uniform.Value.DataType == UniformValueType.Float)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.FloatValue);
                }
                else if (uniform.Value.DataType == UniformValueType.FloatArray)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.FloatArrayValue);
                }
            }

            return skUniforms;
        }
        
        private SKRuntimeEffectChildren UniformsToSkChildren(Uniforms uniforms, SKRuntimeEffect effect)
        {
            SKRuntimeEffectChildren skChildren = new SKRuntimeEffectChildren(effect);
            foreach (var uniform in uniforms)
            {
                if (uniform.Value.DataType == UniformValueType.Shader)
                {
                    skChildren.Add(uniform.Value.Name, this[uniform.Value.ShaderValue.ObjectPointer]);
                }
            }

            return skChildren;
        }
    }
}
