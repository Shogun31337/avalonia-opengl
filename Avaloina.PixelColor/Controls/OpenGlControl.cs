﻿using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Common;
using System;

using static Avalonia.OpenGL.GlConsts;

namespace Avaloina.PixelColor.Controls;

public sealed class OpenGlControl : OpenGlControlBase
{
    private GlExtrasInterface? _glExtras;

    private Int32 _vao;

    private Int32 _vbo;

    private Int32 _ebo;

    private Int32 _program;

    static OpenGlControl()
    {

    }

    public OpenGlControl()
    {
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);

        gl.ClearColor(r: 0.3922f, g: 0.5843f, b: 0.9294f, a: 1);

        _glExtras = new GlExtrasInterface(gl);
        _vao = _glExtras.GenVertexArray();
        _glExtras.BindVertexArray(_vao);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);

        var vertices = Constants.Vertices;
        fixed (Single* buf = vertices)
        {
            gl.BufferData(
                target: GL_ARRAY_BUFFER,
                size: (IntPtr)(vertices.Length * sizeof(Single)),
                data: (IntPtr)buf,
                usage: GL_STATIC_DRAW);
        }

        var indices = Constants.Indices;

        _ebo = gl.GenBuffer();
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);

        fixed (UInt32* buf = indices)
        {
            gl.BufferData(
                target: GL_ELEMENT_ARRAY_BUFFER,
                size: (IntPtr)(indices.Length * sizeof(UInt32)),
                data: (IntPtr)buf,
                usage: GL_STATIC_DRAW);
        }

        var vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
        var error = gl.CompileShaderAndGetError(
            vertexShader,
            ConstantStrings.VertexShader);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        var fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
        error = gl.CompileShaderAndGetError(
            fragmentShader,
            ConstantStrings.FragmentShader);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        _program = gl.CreateProgram();
        gl.AttachShader(_program, vertexShader);
        gl.AttachShader(_program, fragmentShader);

        error = gl.LinkProgramAndGetError(_program);
        if (!String.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        const Int32 positionLoc = 0;
        gl.EnableVertexAttribArray(positionLoc);
        gl.VertexAttribPointer(
            index: positionLoc,
            size: 3, 
            type: GL_FLOAT,
            normalized: 1,
            stride: 3 * sizeof(Single),
            pointer: IntPtr.Zero);

        _glExtras.BindVertexArray(0);
        gl.BindBuffer(GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        base.OnOpenGlDeinit(gl, fb);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        var width = (Int32)Bounds.Width;
        var height = (Int32)Bounds.Height;
        gl.Viewport(0, 0, width, height);

        var glExtras = _glExtras;
        if (glExtras is not null)
        { 
            glExtras.BindVertexArray(_vao);
            gl.UseProgram(_program);
            gl.DrawElements(
                mode: GL_TRIANGLES,
                count: 6,
                type: GL_UNSIGNED_INT,
                indices: IntPtr.Zero);
        }
    }

    private class GlExtrasInterface : GlInterfaceBase<GlInterface.GlContextInfo>
    {
        public GlExtrasInterface(GlInterface gl)
            : base(gl.GetProcAddress, gl.ContextInfo)
        {
        }

        public unsafe delegate void GlGetTexImage(Int32 target, Int32 level, Int32 format, Int32 type, void* pixels);
        [GlMinVersionEntryPoint("glGetTexImage", 3, 0)]
        public GlGetTexImage GetTexImage { get; }

        public unsafe delegate void GlPixelStore(Int32 parameterName, Int32 parameterValue);
        [GlMinVersionEntryPoint("glPixelStorei", 3, 0)]
        public GlPixelStore PixelStore { get; }

        public unsafe delegate void GlReadBuffer(Int32 mode);
        [GlMinVersionEntryPoint("glReadBuffer", 3, 0)]
        public GlReadBuffer ReadBuffer { get; }

        public unsafe delegate void GlReadPixels(Int32 x, Int32 y, Int32 width, Int32 height, Int32 format, Int32 type, void* data);
        [GlMinVersionEntryPoint("glReadPixels", 3, 0)]
        public GlReadPixels ReadPixels { get; }

        public delegate void GlDeleteVertexArrays(Int32 count, Int32[] buffers);
        [GlMinVersionEntryPoint("glDeleteVertexArrays", 3, 0)]
        [GlExtensionEntryPoint("glDeleteVertexArraysOES", "GL_OES_vertex_array_object")]
        public GlDeleteVertexArrays DeleteVertexArrays { get; }

        public delegate void GlBindVertexArray(Int32 array);
        [GlMinVersionEntryPoint("glBindVertexArray", 3, 0)]
        [GlExtensionEntryPoint("glBindVertexArrayOES", "GL_OES_vertex_array_object")]
        public GlBindVertexArray BindVertexArray { get; }
        public delegate void GlGenVertexArrays(Int32 n, Int32[] rv);

        [GlMinVersionEntryPoint("glGenVertexArrays", 3, 0)]
        [GlExtensionEntryPoint("glGenVertexArraysOES", "GL_OES_vertex_array_object")]
        public GlGenVertexArrays GenVertexArrays { get; }

        public Int32 GenVertexArray()
        {
            var rv = new Int32[1];
            GenVertexArrays(1, rv);
            return rv[0];
        }
    }
}