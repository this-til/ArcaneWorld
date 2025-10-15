using System;
using System.IO;
using Godot;
using log4net.Appender;
using log4net.Core;

namespace ArcaneWorld.Util;

public class GodotAppender : AppenderSkeleton {
    
    protected override void Append(LoggingEvent loggingEvent) {
        var message = RenderLoggingEvent(loggingEvent);
        
        // 根据日志级别选择输出方式
        if (loggingEvent.Level >= Level.Error) {
            GD.PrintErr(message);
        } else {
            GD.Print(message);
        }
    }
}
