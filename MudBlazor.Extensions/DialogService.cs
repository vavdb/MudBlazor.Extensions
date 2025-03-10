﻿using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Extensions.Helper;
using MudBlazor.Extensions.Options;
using Nextended.Core;
using Nextended.Core.Extensions;
using Nextended.Core.Helper;

namespace MudBlazor.Extensions
{
    public static partial class DialogServiceExt
    {
        private static DialogParameters MergeParameters(DialogParameters dialogParameters, DialogParameters parameters)
        {
            if (dialogParameters != null)
            {
                foreach (var param in dialogParameters)
                    parameters.Add(param.Key, param.Value);
            }

            return parameters;
        }
        
        public static DialogParameters ToDialogParameters(this IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var dialogParameters = new DialogParameters();
            foreach (var parameter in parameters)
                dialogParameters.Add(parameter.Key, parameter.Value);
            return dialogParameters;
        }

        public static DialogParameters ConvertToDialogParameters<TDialog>(this Action<TDialog> dialogParameters) where TDialog : new()
            => DictionaryHelper.GetValuesDictionary(dialogParameters, true).Where(p => typeof(TDialog).GetProperty(p.Key, BindingFlags.Public | BindingFlags.Instance)?.CanWrite == true).ToDialogParameters();

        public static DialogParameters ConvertToDialogParameters<TDialog>(this TDialog dialogParameters) where TDialog : new()
            => DictionaryHelper.GetValuesDictionary(dialogParameters, true).Where(p => typeof(TDialog).GetProperty(p.Key, BindingFlags.Public | BindingFlags.Instance)?.CanWrite == true).ToDialogParameters();

        public static Task<IDialogReference> ShowEx<TDialog>(this IDialogService dialogService, string title, Action<TDialog> dialogParameters, Action<DialogOptionsEx> optionsEx)
            where TDialog : ComponentBase, new()
        {
            var options = new DialogOptionsEx();
            optionsEx(options);
            return dialogService.ShowEx<TDialog>(title, dialogParameters, options);
        }

        public static Task<IDialogReference> ShowEx<TDialog>(this IDialogService dialogService, string title, TDialog dialogParameters, Action<DialogOptionsEx> optionsEx)
            where TDialog : ComponentBase, new()
        {
            var options = new DialogOptionsEx();
            optionsEx(options);
            return dialogService.ShowEx<TDialog>(title, dialogParameters, options);
        }

        public static async Task<IDialogReference> ShowEx<TDialog>(this IDialogService dialogService, string title, TDialog dialogParameters, DialogOptionsEx optionsEx = null) where TDialog : ComponentBase, new()
            => await dialogService.ShowEx<TDialog>(title, dialogParameters.ConvertToDialogParameters(), optionsEx ?? new DialogOptionsEx());

        public static async Task<IDialogReference> ShowEx<TDialog>(this IDialogService dialogService, string title, Action<TDialog> dialogParameters, DialogOptionsEx optionsEx = null) where TDialog : ComponentBase, new()
            => await dialogService.ShowEx<TDialog>(title, dialogParameters.ConvertToDialogParameters(), optionsEx ?? new DialogOptionsEx());

        public static IDialogReference Show<TDialog>(this IDialogService dialogService, string title, Action<TDialog> dialogParameters, Action<DialogOptions> options)
            where TDialog : ComponentBase, new()
        {
            var dlgOptions = new DialogOptions();
            options(dlgOptions);
            return Show<TDialog>(dialogService, title, dialogParameters, dlgOptions);
        }

        public static IDialogReference Show<TDialog>(this IDialogService dialogService, string title, TDialog dialogParameters, Action<DialogOptions> options)
            where TDialog : ComponentBase, new()
        {
            var dlgOptions = new DialogOptions();
            options(dlgOptions);
            return dialogService.Show<TDialog>(title, dialogParameters, dlgOptions);
        }

        public static IDialogReference Show<TDialog>(this IDialogService dialogService, string title, TDialog dialogParameters, DialogOptions options = null) where TDialog : ComponentBase, new()
            => dialogService.Show<TDialog>(title, dialogParameters.ConvertToDialogParameters(), options ?? new DialogOptions());

        public static IDialogReference Show<TDialog>(this IDialogService dialogService, string title, Action<TDialog> dialogParameters, DialogOptions options = null) where TDialog : ComponentBase, new()
            => dialogService.Show<TDialog>(title, dialogParameters.ConvertToDialogParameters(), options ?? new DialogOptions());

        public static async Task<IDialogReference> ShowEx<T>(this IDialogService dialogService, string title, DialogParameters parameters, DialogOptionsEx options = null) where T : ComponentBase 
            => await dialogService.Show<T>(title, parameters, options).InjectOptionsAsync(options);

        public static async Task<IDialogReference> ShowEx<T>(this IDialogService dialogService, string title, DialogOptionsEx options = null) where T : ComponentBase 
            => await dialogService.Show<T>(title, options).InjectOptionsAsync(options);

        public static async Task<IDialogReference> ShowEx(this IDialogService dialogService, Type type, string title, DialogParameters parameters, DialogOptionsEx options = null) 
            => await dialogService.Show(type, title, parameters, options).InjectOptionsAsync(options);

        public static async Task<bool?> ShowMessageBoxEx(this IDialogService dialogService, MessageBoxOptions mboxOptions, DialogOptionsEx options = null)
        {
            DialogParameters parameters = new DialogParameters()
            {
                ["Title"] = mboxOptions.Title,
                ["Message"] = mboxOptions.Message,
                ["MarkupMessage"] = mboxOptions.MarkupMessage,
                ["CancelText"] = mboxOptions.CancelText,
                ["NoText"] = mboxOptions.NoText,
                ["YesText"] = mboxOptions.YesText
            };
            
            string title = mboxOptions.Title;
            
            DialogResult result = await (await dialogService.Show<MudMessageBox>(title, parameters, options).InjectOptionsAsync(options)).Result;
            return result.Cancelled || !(result.Data is bool data) ? new bool?() : data;
        }

        public static async Task<T> GetDialogAsync<T>(this IDialogReference dialogReference) where T : ComponentBase
        {
            await Task.Run(() =>
            {
                while (dialogReference.Dialog == null)
                    Thread.Sleep(10);
            });
            return dialogReference.Dialog as T;
        }

        public static async Task<IDialogReference> ShowEx(this IDialogService dialogService, Type type, string title, DialogOptionsEx options = null)
        {
            return await dialogService.Show(type, title, options).InjectOptionsAsync(options);
        }

        public static string GetDialogId(this IDialogReference dialogReference)
        {
            return dialogReference != null && dialogReference.Id != Guid.Empty ? $"_{dialogReference.Id.ToString().Replace("-", "")}" : null;
        }

        private static async Task<IDialogReference> InjectOptionsAsync(this IDialogReference dialogReference, DialogOptionsEx options)
        {
            var dialogComponent = await new Func<ComponentBase>(() => dialogReference.Dialog as ComponentBase).WaitForResultAsync();
            DotNetObjectReference<ComponentBase> callbackReference = DotNetObjectReference.Create(dialogComponent);
            var js = await JsImportHelper.GetInitializedJsRuntime(dialogComponent, options.JsRuntime);

            if (options.MinimizeButton == true)
            {
                options.Buttons = InsertButton(options, new MudDialogButton(null, null)
                {
                    Id = $"mud-button-minimize-{Guid.NewGuid()}",
                    Icon = Icons.Filled.Minimize
                });
            }
            if (options.MaximizeButton == true)
            {
                options.Buttons = InsertButton(options, new MudDialogButton(null, null)
                {
                    Id = $"mud-button-maximize-{Guid.NewGuid()}",
                    Icon = Icons.Filled.AspectRatio
                });
            }

            options.Buttons ??= Array.Empty<MudDialogButton>();
            options.Buttons.Apply((i, button) => button.Html = button.GetHtml(i + (options.CloseButton == true ? 1 : 0)));
            await js.InvokeVoidAsync("MudBlazorExtensions.setNextDialogOptions", options, callbackReference);
            return dialogReference;
        }

        private static MudDialogButton[] InsertButton(DialogOptionsEx options, MudDialogButton btn)
        {
            var buttons = options.Buttons?.ToList() ?? new List<MudDialogButton>();
            buttons.Insert(0, btn);
            return buttons.ToArray();
        }

    }
}