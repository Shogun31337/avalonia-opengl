﻿#nullable enable

using Silk.NET.OpenGL;
using System;
using System.IO;
using System.Numerics;

namespace Avalonia.PixelColor.Utils.OpenGl.Silk;

public class Shader : IDisposable
{
    private uint _handle;
    private GL _gl;

    public Shader(GL gl, String vertexPath, String fragmentPath, Boolean loadShadersFromFile = true)
    {
        _gl = gl;

        UInt32 vertex = LoadShader(ShaderType.VertexShader, vertexPath, loadShadersFromFile);
        UInt32 fragment = LoadShader(ShaderType.FragmentShader, fragmentPath, loadShadersFromFile);
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
        }

        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform1(location, value);
    }

    public void SetUniform(String name, Vector4 value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform4(location, value);
    }

    public void SetUniform(String name, Vector3 value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform3(location, value);
    }

    public void SetUniform(String name, Vector2 value)
    {
        var location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }

        _gl.Uniform2(location, value);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    private UInt32 LoadShader(ShaderType type, String shaderSource, Boolean loadShadersFromFile)
    {
        var src = loadShadersFromFile 
            ? File.ReadAllText(shaderSource) 
            : shaderSource;
        var handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        var infoLog = _gl.GetShaderInfoLog(handle);
        if (!String.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}
