﻿using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.PixelColor.Utils.OpenGl;
using Avalonia.PixelColor.Utils.OpenGl.Scenes;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.ColorfulVoronoiScene;
using Avalonia.PixelColor.Utils.OpenGl.Scenes.LinesSilkScene;
using Common;
using System;
using System.Collections.Generic;
using static Avalonia.OpenGL.GlConsts;

namespace Avalonia.PixelColor.Controls;

public sealed class OpenGlControl : OpenGlControlBase
{
    private GlInterface? _gl;

    static OpenGlControl()
    {
    }

    public OpenGlControl()
    {
        Scene = new Lines4Scene(GlVersion);
    }

    public IOpenGlScene Scene { get; private set; }

    private IOpenGlScene? _nextScene;

    public IEnumerable<OpenGlSceneParameter> ChangeScene(OpenGlScenesEnum scene)
    {
        var parameters = Scene.Parameters;
        if (scene != Scene.Scene)
        {
            IOpenGlScene nextScene = scene switch
            {
                OpenGlScenesEnum.Rectangle => new RectangleScene(),
                OpenGlScenesEnum.Lines => new LinesScene(GlVersion),
                OpenGlScenesEnum.Lines2 => new Lines2Scene(GlVersion),
                OpenGlScenesEnum.Lines3 => new Lines3Scene(GlVersion),
                OpenGlScenesEnum.Lines4 => new Lines4Scene(GlVersion),
                OpenGlScenesEnum.ColorfulVoronoi => new ColorfulVoronoiScene(GlVersion),
                OpenGlScenesEnum.LinesSilk => new LinesSilkScene(),
                OpenGlScenesEnum.ISFScene => new ISFScene(GlVersion),
                _ => new RectangleScene(),
            };
            _nextScene = nextScene;
            parameters = nextScene.Parameters;
        }

        return parameters;
    }

    public IEnumerable<OpenGlSceneParameter> ChangeScene(IOpenGlScene scene)
    {
        _nextScene = scene;
        var parameters = scene.Parameters;
        return parameters;
    }

    private void SelectScene()
    {
        var nextScene = _nextScene;
        if (nextScene is not null)
        {
            var gl = _gl;
            if (gl is not null)
            {
                var previousScene = Scene;
                previousScene?.DeInitialize(gl);

                nextScene.Initialize(gl);
                Scene = nextScene;
            }
        }
    }

    public Double ScaleFactor { get; set; } = 1;

    public List<TrackPoint> TrackPoints
    {
        get;
    } = new List<TrackPoint>();    

    private unsafe void GetTrackPointsColors(Int32 width, Int32 height)
    {
        var gl = _gl;
        if (gl is not null)
        {
            foreach (var trackPoint in TrackPoints)
            {
                var x = (Int32)Math.Round(trackPoint.ReleativeX * width);
                var y = (Int32)Math.Round(trackPoint.ReleativeY * height);
                var pixels = new Byte[OpenGlConstants.RgbaSize];
                fixed (void* pPixels = pixels)
                {
                    gl.ReadPixels(
                        x: x,
                        y: y,
                        width: 1,
                        height: 1,
                        format: GL_RGBA,
                        type: GL_UNSIGNED_BYTE,
                        data: pPixels);
                }

                var r = pixels[0];
                var g = pixels[1];
                var b = pixels[2];
                var a = pixels[3];
                var color = System.Drawing.Color.FromArgb(a, r, g, b);
                trackPoint.Color = color;
            }
        }
    }

    private unsafe void DoScreenShot(
        String fullName,
        Int32 width,
        Int32 height,
        Double scaleFactor)
    {
        var gl = _gl;
        if (gl is not null )
        {
            var pixelSize = OpenGlConstants.RgbaSize;
            var newPixelSize = (Int32)(pixelSize * scaleFactor);
            var pixelsCount = (Int32)newPixelSize * width * height;
            var pixels = new Byte[pixelsCount];
            gl.Finish();
            fixed (void* pPixels = pixels)
            {
                gl.ReadPixels(
                    x: 0,
                    y: 0,
                    width: width,
                    height: height,
                    format: GL_RGBA,
                    type: GL_UNSIGNED_BYTE,
                    data: pPixels);
            }

            Common.Utils.SaveScreenshot(
                rgbaData: pixels, 
                width: width,
                height: height,
                pixelSize: pixelSize,
                filename: fullName);
        }
    }

    private String _screenShotFullName = String.Empty;

    public void MakeScreenShot(String fullName)
    {
        _screenShotFullName = fullName;
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl)
    {
        _gl = gl;
        base.OnOpenGlInit(gl);
        Scene.Initialize(gl);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        _gl = gl;
        base.OnOpenGlDeinit(gl);
        Scene.DeInitialize(gl);
        gl.UseProgram(0);
    }

    protected override void OnOpenGlRender(GlInterface gl, Int32 fb)
    {
        _gl = gl;
        SelectScene();
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        var width = (Int32)Bounds.Width;
        var height = (Int32)Bounds.Height;
        var scaleFactor = ScaleFactor;
        var finalWidth = (Int32)(width * scaleFactor);
        var finalHeight = (Int32)(height * scaleFactor);
        gl.Viewport(0, 0, finalWidth, finalHeight);
        Scene.Render(gl, finalWidth, finalHeight);
        gl.Flush();

        var screenShotFullName = _screenShotFullName;
        _screenShotFullName = String.Empty;
        if (!String.IsNullOrEmpty(screenShotFullName))
        {
            DoScreenShot(
                fullName: screenShotFullName,
                width: finalWidth,
                height: finalHeight,
                scaleFactor: scaleFactor);
        }
    }    
}