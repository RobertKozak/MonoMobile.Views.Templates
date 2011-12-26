// 
//  ProgressButtonAttribute.cs
// 
//  Author:
//    Robert Kozak (rkozak@gmail.com / Twitter:@robertkozak)
// 
//  Copyright 2011, Nowcom Corporation.
// 
//  Code licensed under the MIT X11 license
// 
//  Permission is hereby granted, free of charge, to any person obtaining
//  a copy of this software and associated documentation files (the
//  "Software"), to deal in the Software without restriction, including
//  without limitation the rights to use, copy, modify, merge, publish,
//  distribute, sublicense, and/or sell copies of the Software, and to
//  permit persons to whom the Software is furnished to do so, subject to
//  the following conditions:
// 
//  The above copyright notice and this permission notice shall be
//  included in all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//  WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
namespace MonoMobile.Views
{	
	using System;
	using System.Drawing;
	using MonoTouch.Foundation;
	using MonoTouch.UIKit;
	
	public enum IndicatorStyle
	{
		Hud, 
		AccessoryIndicator
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class ProgressButtonAttribute : LoadMoreAttribute
	{
		public override Type CellViewType { get { return typeof(ProgressCellView); } }

		public ProgressButtonAttribute(): this(null, null)
		{
		}

		public ProgressButtonAttribute(string canExecutePropertyName): this(null, null)
		{
			CanExecutePropertyName = canExecutePropertyName;
			CommandOption = CommandOption.Hide;
		}

		public ProgressButtonAttribute(string title, string detailText)
		{
			Title = title;
			DetailText = detailText;
			IndicatorStyle = IndicatorStyle.Hud;
		}

		public string DetailText { get; set;}
		public IndicatorStyle IndicatorStyle { get; set; }

		[Preserve(AllMembers = true)]
		class ProgressCellView : LoadMoreCellView
		{
			private ProgressButtonAttribute ProgressButtonData { get { return (ProgressButtonAttribute)CellViewTemplate; } }

			public ProgressCellView(RectangleF frame) : base(frame)
			{
			}
			
			public override void UpdateCell(UITableViewCell cell, NSIndexPath indexPath)
			{
				cell.TextLabel.Text = Caption;
				cell.TextLabel.TextAlignment = UITextAlignment.Center;
				cell.Accessory = UITableViewCellAccessory.None;
			}

			public override void Selected(DialogViewController controller, UITableView tableView, object item, NSIndexPath indexPath)
			{
				if (ProgressButtonData.IndicatorStyle == IndicatorStyle.Hud)
				{
					var progress = new ProgressHud() 
					{ 
						TitleText = ProgressButtonData.Title,
						DetailText = ProgressButtonData.DetailText,
						MinimumShowTime = ProgressButtonData.MinimumShowTime,
						GraceTime = ProgressButtonData.GraceTime	
					};
	
					Action asyncCompleted = null;
				
					asyncCompleted = progress.ShowWhileAsync(() =>
					{
						if (DataContext != null)
						{
							InvokeOnMainThread(() =>
							{
								ExecuteMethod(asyncCompleted);
							});
						}	
					}, true);
				}
				else
				{
					base.Selected(controller, tableView, item, indexPath);
				}

				tableView.DeselectRow(indexPath, true);
			}
		}
	}
}

