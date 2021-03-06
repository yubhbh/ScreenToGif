﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ScreenToGif.ImageUtil;
using ScreenToGif.Util;
using ScreenToGif.Windows.Other;
using Clipboard = System.Windows.Clipboard;

namespace ScreenToGif.Controls
{
    /// <summary>
    /// ListViewItem used by the Encoder window.
    /// </summary>
    public class EncoderListViewItem : ListViewItem
    {
        #region Dependency Properties

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(UIElement), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register("Percentage", typeof(double), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty CurrentFrameProperty = DependencyProperty.Register("CurrentFrame", typeof(int), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty FrameCountProperty = DependencyProperty.Register("FrameCount", typeof(int), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(int), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(-1));

        public static readonly DependencyProperty TokenProperty = DependencyProperty.Register("Token", typeof(CancellationTokenSource), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(Status), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(Status.Encoding));

        public static readonly DependencyProperty OutputTypeProperty = DependencyProperty.Register("OutputType", typeof(Export), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(Export.Gif));

        public static readonly DependencyProperty SizeInBytesProperty = DependencyProperty.Register("SizeInBytes", typeof(long), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(0L));

        public static readonly DependencyProperty OutputPathProperty = DependencyProperty.Register("OutputPath", typeof(string), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty OutputFilenameProperty = DependencyProperty.Register("OutputFilename", typeof(string), typeof(EncoderListViewItem),
                new FrameworkPropertyMetadata(OutputFilename_PropertyChanged));

        public static readonly DependencyProperty ExceptionProperty = DependencyProperty.Register("Exception", typeof(Exception), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata());


        public static readonly DependencyProperty UploadedProperty = DependencyProperty.Register("Uploaded", typeof(bool), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty UploadLinkProperty = DependencyProperty.Register("UploadLink", typeof(string), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty UploadTaskExceptionProperty = DependencyProperty.Register("UploadTaskException", typeof(Exception), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(null));


        public static readonly DependencyProperty CopiedToClipboardProperty = DependencyProperty.Register("CopiedToClipboard", typeof(bool), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CopyTaskExceptionProperty = DependencyProperty.Register("CopyTaskException", typeof(Exception), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(null));


        public static readonly DependencyProperty CommandExecutedProperty = DependencyProperty.Register("CommandExecuted", typeof(bool), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty CommandTaskExceptionProperty = DependencyProperty.Register("CommandTaskException", typeof(Exception), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CommandOutputProperty = DependencyProperty.Register("CommandOutput", typeof(string), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(null));


        public static readonly DependencyProperty WillCopyToClipboardProperty = DependencyProperty.Register("WillCopyToClipboard", typeof(bool), typeof(EncoderListViewItem),
            new FrameworkPropertyMetadata(false));

        #endregion

        #region Properties

        /// <summary>
        /// The Image of the ListViewItem.
        /// </summary>
        [Description("The Image of the ListViewItem.")]
        public UIElement Image
        {
            get => (UIElement)GetValue(ImageProperty);
            set => SetCurrentValue(ImageProperty, value);
        }

        /// <summary>
        /// The encoding percentage.
        /// </summary>
        [Description("The encoding percentage.")]
        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            set => SetCurrentValue(PercentageProperty, value);
        }

        /// <summary>
        /// The current frame being processed.
        /// </summary>
        [Description("The frame count.")]
        public int CurrentFrame
        {
            get => (int)GetValue(CurrentFrameProperty);
            set
            {
                SetCurrentValue(CurrentFrameProperty, value);

                if (CurrentFrame == 0)
                {
                    Percentage = 0;
                    return;
                }

                // 100% = FrameCount
                // 100% * CurrentFrame / FrameCount = Actual Percentage
                Percentage = Math.Round(CurrentFrame * 100.0 / FrameCount, 1, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// The frame count.
        /// </summary>
        [Description("The frame count.")]
        public int FrameCount
        {
            get => (int)GetValue(FrameCountProperty);
            set => SetCurrentValue(FrameCountProperty, value);
        }

        /// <summary>
        /// The description of the item.
        /// </summary>
        [Description("The description of the item.")]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetCurrentValue(TextProperty, value);
        }

        /// <summary>
        /// The ID of the Task.
        /// </summary>
        [Description("The ID of the Task.")]
        public int Id
        {
            get => (int)GetValue(IdProperty);
            set => SetCurrentValue(IdProperty, value);
        }

        /// <summary>
        /// The Cancellation Token Source.
        /// </summary>
        [Description("The Cancellation Token Source.")]
        public CancellationTokenSource TokenSource
        {
            get => (CancellationTokenSource)GetValue(TokenProperty);
            set => SetCurrentValue(TokenProperty, value);
        }

        /// <summary>
        /// The state of the progress bar.
        /// </summary>
        [Description("The state of the progress bar.")]
        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetCurrentValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        /// The status of the encoding.
        /// </summary>
        [Description("The status of the encoding.")]
        public Status Status
        {
            get => (Status)GetValue(StatusProperty);
            set => SetCurrentValue(StatusProperty, value);
        }

        /// <summary>
        /// The size of the output file in bytes.
        /// </summary>
        [Description("The size of the output file in bytes.")]
        public long SizeInBytes
        {
            get => (long)GetValue(SizeInBytesProperty);
            set => SetCurrentValue(SizeInBytesProperty, value);
        }

        /// <summary>
        /// The filename of the output file.
        /// </summary>
        [Description("The filename of the output file.")]
        public string OutputFilename
        {
            get => (string)GetValue(OutputFilenameProperty);
            set => SetCurrentValue(OutputFilenameProperty, value);
        }

        /// <summary>
        /// The path of the output file.
        /// </summary>
        [Description("The path of the output file.")]
        public string OutputPath
        {
            get => (string)GetValue(OutputPathProperty);
            set => SetCurrentValue(OutputPathProperty, value);
        }

        /// <summary>
        /// The type of the output.
        /// </summary>
        [Description("The type of the output.")]
        public Export OutputType
        {
            get => (Export)GetValue(OutputTypeProperty);
            set => SetCurrentValue(OutputTypeProperty, value);
        }

        /// <summary>
        /// The exception of the encoding.
        /// </summary>
        [Description("The exception of the encoding.")]
        public Exception Exception
        {
            get => (Exception)GetValue(ExceptionProperty);
            set => SetCurrentValue(ExceptionProperty, value);
        }


        /// <summary>
        /// True if the outfile file was uploaded.
        /// </summary>
        [Description("True if the outfile file was uploaded.")]
        public bool Uploaded
        {
            get => (bool)GetValue(UploadedProperty);
            set => SetCurrentValue(UploadedProperty, value);
        }

        /// <summary>
        /// The link to the uploaded file.
        /// </summary>
        [Description("The link to the uploaded file.")]
        public string UploadLink
        {
            get => (string)GetValue(UploadLinkProperty);
            set => SetCurrentValue(UploadLinkProperty, value);
        }

        /// <summary>
        /// The exception detail about the upload task.
        /// </summary>
        [Description("The exception detail about the upload task.")]
        public Exception UploadTaskException
        {
            get => (Exception)GetValue(UploadTaskExceptionProperty);
            set => SetCurrentValue(UploadTaskExceptionProperty, value);
        }



        /// <summary>
        /// True if the outfile file was copied to the clipboard.
        /// </summary>
        [Description("True if the outfile file was copied to the clipboard.")]
        public bool CopiedToClipboard
        {
            get => (bool)GetValue(CopiedToClipboardProperty);
            set => SetCurrentValue(CopiedToClipboardProperty, value);
        }

        /// <summary>
        /// The exception detail about the copy task.
        /// </summary>
        [Description("The exception detail about the copy task.")]
        public Exception CopyTaskException
        {
            get => (Exception)GetValue(CopyTaskExceptionProperty);
            set => SetCurrentValue(CopyTaskExceptionProperty, value);
        }



        /// <summary>
        /// True if the post encoding commands were executed.
        /// </summary>
        [Description("True if the post encoding commands were executed.")]
        public bool CommandExecuted
        {
            get => (bool)GetValue(CommandExecutedProperty);
            set => SetCurrentValue(CommandExecutedProperty, value);
        }

        /// <summary>
        /// The exception detail about the post encoding command task.
        /// </summary>
        [Description("The exception detail about the post encoding command task.")]
        public Exception CommandTaskException
        {
            get => (Exception)GetValue(CommandTaskExceptionProperty);
            set => SetCurrentValue(CommandTaskExceptionProperty, value);
        }

        /// <summary>
        /// The output from the post encoding commands.
        /// </summary>
        [Description("The output from the post encoding commands.")]
        public string CommandOutput
        {
            get => (string)GetValue(CommandOutputProperty);
            set => SetCurrentValue(CommandOutputProperty, value);
        }


        /// <summary>
        /// True if the process will copy the final file to the clipboard.
        /// </summary>
        [Description("True if the process will copy the final file to the clipboard.")]
        public bool WillCopyToClipboard
        {
            get => (bool)GetValue(WillCopyToClipboardProperty);
            set => SetCurrentValue(WillCopyToClipboardProperty, value);
        }

        #endregion

        #region Custom Events

        public static readonly RoutedEvent CancelClickedEvent = EventManager.RegisterRoutedEvent("CancelClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EncoderListViewItem));

        public static readonly RoutedEvent OpenFileClickedEvent = EventManager.RegisterRoutedEvent("OpenFileClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EncoderListViewItem));

        public static readonly RoutedEvent ExploreFolderClickedEvent = EventManager.RegisterRoutedEvent("ExploreFolderClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EncoderListViewItem));

        /// <summary>
        /// Event raised when the user clicks on the cancel button.
        /// </summary>
        public event RoutedEventHandler CancelClicked
        {
            add => AddHandler(CancelClickedEvent, value);
            remove => RemoveHandler(CancelClickedEvent, value);
        }

        /// <summary>
        /// Event raised when the user clicks on the "Open file" button.
        /// </summary>
        public event RoutedEventHandler OpenFileClicked
        {
            add => AddHandler(OpenFileClickedEvent, value);
            remove => RemoveHandler(OpenFileClickedEvent, value);
        }

        /// <summary>
        /// Event raised when the user clicks on the "Explore folder" button.
        /// </summary>
        public event RoutedEventHandler ExploreFolderClicked
        {
            add => AddHandler(ExploreFolderClickedEvent, value);
            remove => RemoveHandler(ExploreFolderClickedEvent, value);
        }

        public void RaiseCancelClickedEvent()
        {
            if (CancelClickedEvent == null || !IsLoaded)
                return;

            var newEventArgs = new RoutedEventArgs(CancelClickedEvent);
            RaiseEvent(newEventArgs);
        }

        public void RaiseOpenFileClickedEvent()
        {
            if (OpenFileClickedEvent == null || !IsLoaded)
                return;

            var newEventArgs = new RoutedEventArgs(OpenFileClickedEvent);
            RaiseEvent(newEventArgs);
        }

        public void RaiseExploreFolderClickedEvent()
        {
            if (ExploreFolderClickedEvent == null || !IsLoaded)
                return;

            var newEventArgs = new RoutedEventArgs(ExploreFolderClickedEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        static EncoderListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EncoderListViewItem), new FrameworkPropertyMetadata(typeof(EncoderListViewItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var cancelButton = Template.FindName("CancelButton", this) as ImageButton;
            var fileButton = Template.FindName("FileButton", this) as ImageButton;
            var folderButton = Template.FindName("FolderButton", this) as ImageButton;
            var detailsButton = Template.FindName("DetailsButton", this) as ImageButton;
            var copyMenu = Template.FindName("CopyMenuItem", this) as ImageMenuItem;
            var copyImageMenu = Template.FindName("CopyImageMenuItem", this) as ImageMenuItem;
            var copyFilenameMenu = Template.FindName("CopyFilenameMenuItem", this) as ImageMenuItem;
            var copyFolderMenu = Template.FindName("CopyFolderMenuItem", this) as ImageMenuItem;

            if (cancelButton != null)
                cancelButton.Click += (s, a) => RaiseCancelClickedEvent();

            //Open file.
            if (fileButton != null)
                fileButton.Click += (s, a) =>
                {
                    RaiseOpenFileClickedEvent();

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(OutputFilename) && File.Exists(OutputFilename))
                            Process.Start(OutputFilename);
                    }
                    catch (Exception ex)
                    {
                        Dialog.Ok("Open File", "Error while openning the file", ex.Message);
                    }
                };

            //Open folder.
            if (folderButton != null)
                folderButton.Click += (s, a) =>
                {
                    RaiseExploreFolderClickedEvent();

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(OutputFilename) && Directory.Exists(OutputPath))
                            Process.Start("explorer.exe", $"/select,\"{OutputFilename.Replace("/","\\")}\"");
                    }
                    catch (Exception ex)
                    {
                        Dialog.Ok("Explore Folder", "Error while opening the folder", ex.Message);
                    }
                };

            //Details. Usually when something wrong happens.
            if (detailsButton != null)
                detailsButton.Click += (s, a) =>
                {
                    if (Exception != null)
                    {
                        var viewer = new ExceptionViewer(Exception);
                        viewer.ShowDialog();
                    }  
                };

            //Copy (as image and text).
            if (copyMenu != null)
                copyMenu.Click += (s, a) => 
                {
                    if (!string.IsNullOrWhiteSpace(OutputFilename))
                    {
                        var data = new DataObject();
                        data.SetImage(OutputFilename.SourceFrom());
                        data.SetText(OutputFilename, TextDataFormat.Text);
                        data.SetFileDropList(new StringCollection { OutputFilename });

                        Clipboard.SetDataObject(data, true);
                    }
                };

            //Copy as image.
            if (copyImageMenu != null)
                copyImageMenu.Click += (s, a) =>
                {
                    if (!string.IsNullOrWhiteSpace(OutputFilename))
                        Clipboard.SetImage(OutputFilename.SourceFrom());
                };

            //Copy full path.
            if (copyFilenameMenu != null)
                copyFilenameMenu.Click += (s, a) =>
                {
                    if (!string.IsNullOrWhiteSpace(OutputFilename))
                        Clipboard.SetText(OutputFilename);
                };

            //Copy folder path.
            if (copyFolderMenu != null)
                copyFolderMenu.Click += (s, a) =>
                {
                    if (!string.IsNullOrWhiteSpace(OutputPath))
                        Clipboard.SetText(OutputPath);
                };
        }

        private static void OutputFilename_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is EncoderListViewItem item))
                return;

            item.OutputPath = Path.GetDirectoryName(item.OutputFilename);
        }
    }
}