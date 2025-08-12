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
            socket.OnOpen = () => Trace.WriteLine("�ͻ������Ӵ�");
            socket.OnClose = () => Trace.WriteLine("�ͻ������ӹر�");
            socket.OnMessage = message =>
            {
                Trace.WriteLine("�յ���Ϣ: " + message);
                socket.Send("Echo: " + message);
            };
        });


        base.OnFrameworkInitializationCompleted();
    }

}