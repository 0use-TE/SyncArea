using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Fleck;
using MudBlazorCodeGenerator.ViewModels;
using MudBlazorCodeGenerator.Views;
using System.Diagnostics;

namespace MudBlazorCodeGenerator;

public partial class App : Application
{
    private WebSocketServer socketServer;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        socketServer = new WebSocketServer("ws://localhost:8181");

        socketServer.Start(socket =>
        {
            socket.OnOpen = () => Trace.WriteLine("客户端连接打开");
            socket.OnClose = () => Trace.WriteLine("客户端连接关闭");
            socket.OnMessage = message =>
            {
                Trace.WriteLine("收到消息: " + message);
                socket.Send("Echo: " + message);
            };
        });


        base.OnFrameworkInitializationCompleted();
    }

}