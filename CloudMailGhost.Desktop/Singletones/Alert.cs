using MsBox.Avalonia;
using System;
using MsBox.Avalonia.Enums;
using Avalonia.Controls;

namespace CloudMailGhost.Desktop.Singletones
{
    internal static class Alert
    {
        internal async static void ShowError(this Window ownerWindow, Exception e)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Ошыбка", $"{e.Message}\n\nДля разработчика: {e.GetType().Name}, {e.StackTrace}", ButtonEnum.Ok, Icon.Error);

            var result = await box.ShowWindowDialogAsync(ownerWindow);
        }

        internal async static void ShowMessage(this Window ownerWindow, string msg)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Внимание", msg, ButtonEnum.Ok, Icon.Info);

            var result = await box.ShowWindowDialogAsync(ownerWindow);
        }
    }
}
