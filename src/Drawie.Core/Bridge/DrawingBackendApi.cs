﻿using Drawie.Core.Exceptions;

namespace Drawie.Core.Bridge
{
    public static class DrawingBackendApi
    {
        private static IDrawingBackend? _current;

        public static IDrawingBackend Current
        {
            get
            {
                if (_current == null)
                    throw new NullReferenceException("Either drawing backend was not yet initialized or reference was somehow lost.");

                return _current;
            }
        }
        
        public static bool HasBackend => _current != null;
        
        public static void SetupBackend(IDrawingBackend backend, IRenderingServer server)
        {
            if (_current != null)
            {
                throw new InitializationDuplicateException("Drawing backend was already initialized.");
            }
            
            _current = backend;
            _current.RenderingServer = server;
            backend.Setup();
        }
    }
}
