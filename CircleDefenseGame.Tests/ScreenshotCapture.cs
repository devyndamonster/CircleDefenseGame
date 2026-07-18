using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;

namespace CircleDefenseGame.Tests;

internal static class ScreenshotCapture
{
    private const int DwmaExtendedFrameBounds = 9;
    private static readonly Guid GraphicsCaptureItemGuid =
        new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    public static Bitmap CaptureGameScreenshot(Process game)
    {
        if (game.HasExited)
        {
            throw new InvalidOperationException(
                $"The game exited with code {game.ExitCode} before its screenshot was captured.");
        }

        return CaptureWindowScreenshotAsync(game.MainWindowHandle, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    private static async Task<Bitmap> CaptureWindowScreenshotAsync(
        IntPtr windowHandle,
        CancellationToken cancellationToken)
    {
        SetProcessDpiAwarenessContext(new IntPtr(-4));
        Rectangle clientBounds = GetClientBoundsInWindow(windowHandle);
        using CanvasBitmap capturedFrame = await CaptureWindowAsync(windowHandle, cancellationToken);
        int width = (int)capturedFrame.Size.Width;
        int height = (int)capturedFrame.Size.Height;

        if (clientBounds.Left < 0
            || clientBounds.Top < 0
            || clientBounds.Right > width
            || clientBounds.Bottom > height)
        {
            throw new InvalidOperationException(
                $"The client bounds {clientBounds} are outside the captured frame {width}x{height}.");
        }

        CanvasDevice device = CanvasDevice.GetSharedDevice();
        using var clientFrame = new CanvasRenderTarget(
            device,
            clientBounds.Width,
            clientBounds.Height,
            96);
        using (CanvasDrawingSession drawingSession = clientFrame.CreateDrawingSession())
        {
            drawingSession.DrawImage(
                capturedFrame,
                -clientBounds.Left,
                -clientBounds.Top,
                new Rect(0, 0, width, height),
                1,
                CanvasImageInterpolation.NearestNeighbor);
        }

        byte[] pixels = clientFrame.GetPixelBytes();
        SetPixelsOpaque(pixels);
        using var clientScreenshot = new Bitmap(
            clientBounds.Width,
            clientBounds.Height,
            PixelFormat.Format32bppPArgb);
        BitmapData bitmapData = clientScreenshot.LockBits(
            new Rectangle(0, 0, clientBounds.Width, clientBounds.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppPArgb);

        try
        {
            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
        }
        finally
        {
            clientScreenshot.UnlockBits(bitmapData);
        }

        return new Bitmap(clientScreenshot);
    }

    private static void SetPixelsOpaque(byte[] pixels)
    {
        for (int index = 0; index < pixels.Length; index += 4)
        {
            pixels[index + 3] = byte.MaxValue;
        }
    }

    private static async Task<CanvasBitmap> CaptureWindowAsync(
        IntPtr windowHandle,
        CancellationToken cancellationToken)
    {
        if (!GraphicsCaptureSession.IsSupported())
        {
            throw new PlatformNotSupportedException(
                "Windows Graphics Capture is not supported in this environment.");
        }

        GraphicsCaptureItem item = CreateItemForWindow(windowHandle);
        CanvasDevice device = CanvasDevice.GetSharedDevice();
        using Direct3D11CaptureFramePool framePool =
            Direct3D11CaptureFramePool.CreateFreeThreaded(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                1,
                item.Size);
        using GraphicsCaptureSession session = framePool.CreateCaptureSession(item);
        session.IsCursorCaptureEnabled = false;
        var frameCompletion = new TaskCompletionSource<CanvasBitmap>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            try
            {
                using Direct3D11CaptureFrame frame = sender.TryGetNextFrame();
                frameCompletion.TrySetResult(
                    CanvasBitmap.CreateFromDirect3D11Surface(device, frame.Surface));
            }
            catch (Exception exception)
            {
                frameCompletion.TrySetException(exception);
            }
        }

        framePool.FrameArrived += OnFrameArrived;
        session.StartCapture();

        try
        {
            CanvasBitmap capturedFrame = await frameCompletion.Task.WaitAsync(cancellationToken);
            await Task.Yield();
            return capturedFrame;
        }
        finally
        {
            framePool.FrameArrived -= OnFrameArrived;
        }
    }

    private static GraphicsCaptureItem CreateItemForWindow(IntPtr windowHandle)
    {
        IGraphicsCaptureItemInterop interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
        IntPtr itemPointer = interop.CreateForWindow(windowHandle, GraphicsCaptureItemGuid);

        try
        {
            return GraphicsCaptureItem.FromAbi(itemPointer);
        }
        finally
        {
            Marshal.Release(itemPointer);
        }
    }

    private static Rectangle GetClientBoundsInWindow(IntPtr windowHandle)
    {
        if (DwmGetWindowAttribute(
                windowHandle,
                DwmaExtendedFrameBounds,
                out NativeRectangle windowBounds,
                Marshal.SizeOf<NativeRectangle>()) != 0
            || !GetClientRect(windowHandle, out NativeRectangle clientBounds))
        {
            throw new InvalidOperationException("Could not determine the game window bounds.");
        }

        var clientOrigin = new NativePoint();
        if (!ClientToScreen(windowHandle, ref clientOrigin))
        {
            throw new InvalidOperationException("Could not determine the game client origin.");
        }

        return new Rectangle(
            clientOrigin.X - windowBounds.Left,
            clientOrigin.Y - windowBounds.Top,
            clientBounds.Right - clientBounds.Left,
            clientBounds.Bottom - clientBounds.Top);
    }

    [ComImport]
    [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IGraphicsCaptureItemInterop
    {
        IntPtr CreateForWindow(IntPtr windowHandle, ref Guid itemInterfaceId);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        out NativeRectangle value,
        int valueSize);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(IntPtr windowHandle, out NativeRectangle rectangle);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClientToScreen(IntPtr windowHandle, ref NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiAwarenessContext);

    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
