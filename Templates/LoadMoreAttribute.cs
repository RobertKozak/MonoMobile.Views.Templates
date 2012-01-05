//
// LoadMoreAttribute.cs:
//
// Author:
//   Robert Kozak (rkozak@gmail.com / Twitter:@robertkozak
//
// Copyright 2011, Nowcom Corportation
//
// Code licensed under the MIT X11 license
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace MonoMobile.Views
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Reflection;
	using System.Threading;
	using MonoTouch.Foundation;
	using MonoTouch.UIKit;
	
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class LoadMoreAttribute : ButtonAttribute, ISizeable
	{
		public override Type CellViewType { get { return typeof(LoadMoreCellView); } }

		public LoadMoreAttribute(): this(null)
		{
		}

		public LoadMoreAttribute(string title) : base(title)
		{
		}
		
		public string Title { get; set; }
		public float GraceTime { get; set; }
		public float MinimumShowTime { get; set; }
		public int RowHeight { get; set; }

		public float GetRowHeight()
		{
			return RowHeight;
		}	
	}

	[Preserve(AllMembers = true)]
	public class LoadMoreCellView : CellView<MemberInfo>, ISelectable, ICellContent
	{
		private LoadMoreAttribute LoadMoreData { get { return (LoadMoreAttribute)CellViewTemplate; } }
			
		private readonly UIActivityIndicatorView _ActivityIndicator;
		private const int _TopPosition = 11;
		private const int _IndicatorSize = 20;
		private DateTime? _ShowStarted;
		private NSTimer _Timer;

		public UIView CellContentView { get; set; }
		public UIView CellBackgroundView { get; set; }
		public UIView CellSelectedBackgroundView { get; set; }

		public LoadMoreCellView(RectangleF frame) : base(frame)
		{
			_ActivityIndicator = new UIActivityIndicatorView(new RectangleF((_IndicatorSize * 2) - 4, _TopPosition, _IndicatorSize, _IndicatorSize))
				{
					ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White,
					Hidden = true,
					Tag = 1, 
				};

			_ActivityIndicator.StopAnimating();

			CellContentView = _ActivityIndicator;
			Add(CellContentView);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_ActivityIndicator != null)
				{
					_ActivityIndicator.Dispose();
				}

				if (_Timer != null)
				{
					_Timer.Dispose();
					_Timer = null;
				}
			}

			base.Dispose(disposing);
		}
			
		public override void UpdateCell(UITableViewCell cell, NSIndexPath indexPath)
		{
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = UITextAlignment.Center;
			cell.Accessory = UITableViewCellAccessory.None;

			cell.TextLabel.TextColor = Theme.TextColor;
		}

		public virtual void Selected(DialogViewController controller, UITableView tableView, object item, NSIndexPath indexPath)
		{
			if (Animating)
			{
				return;
			}
			
			Animating = true;

			var executeCommandThread = new Thread(ExecuteCommandThread as ThreadStart);
			executeCommandThread.Start();

			tableView.DeselectRow(indexPath, true);
		}
			
		protected virtual void ExecuteMethod(Action asyncCompleted)
		{
			var method = DataContext.Member as MethodInfo;
			if (method != null)
			{	
				var parameters = method.GetParameters();
				if (parameters == null || parameters.Length == 0)
				{
					method.Invoke(DataContext.Source, null);
					asyncCompleted();
				}
				else if (parameters != null && parameters.Last().ParameterType == typeof(Action))
				{
				  	object[] methodParams = new object[parameters.Length];
					methodParams[parameters.Length - 1] = asyncCompleted;
					method.Invoke(DataContext.Source, methodParams);
				}
			}
		}

		private void ExecuteCommandThread()
		{
			using (var pool = new NSAutoreleasePool())
			{
				pool.InvokeOnMainThread(delegate
				{
					ExecuteMethod(() => Animating = false);
				});
			}
		}
			
		private void StartActivityIndicator()
		{
			if (Cell != null)
			{
				Cell.TextLabel.Text = LoadMoreData.Title;
			}
				
			_ActivityIndicator.Hidden = false;
			_ActivityIndicator.StartAnimating();
		}

		private void StopActivityIndicator()
		{
			_ActivityIndicator.StopAnimating();
			_ActivityIndicator.Hidden = true;

			if (Cell != null)
			{
				Cell.TextLabel.Text = Caption;
			}
		}

		public bool Animating
		{
			get
			{
				return _ActivityIndicator.IsAnimating;
			}
			set
			{
				// Doesn't work due to a bug in MonoTouch that should be fixed in 4.0 
				// float hue, saturation, brightness, alpha;
				// Cell.TextLabel.TextColor.GetHSBA(out hue, out saturation, out brightness, out alpha);

				// Check to see if Cell exists. When calling asynchronously its possible the RootView was dismissed 
				// before this finishes so the Cell would be disposed. This can happen for a Login Button that calls a 
				// webservice and dismisses the Login View when successful. In this case by the time we come here the Cell
				// has already been dismissed.
				if (Cell != null)
				{
					var textColor = Cell.TextLabel.TextColor;
					var brightness = 1 - textColor.CGColor.Components[0];					
	
					if (brightness > 0.5f)
					{
						_ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
					}
					else
					{
						_ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
					}
				}

				if (value)
				{
					_ShowStarted = DateTime.Now;
					// If the grace time is set postpone the ActivityIndicator
					if (LoadMoreData.GraceTime > 0.0)
					{
						_Timer = NSTimer.CreateScheduledTimer(LoadMoreData.GraceTime, StartActivityIndicator);
					}
					else
					{
						StartActivityIndicator();
					}
				}
				else
				{
					// If the minShow time is set, calculate how long the ActivityIndicator was shown,
					// and pospone the hiding operation if necessary
					if (LoadMoreData.MinimumShowTime > 0.0 && _ShowStarted.HasValue)
					{
						double interv = (DateTime.Now - _ShowStarted.Value).TotalSeconds;
						if (interv < LoadMoreData.MinimumShowTime)
						{
							_Timer = NSTimer.CreateScheduledTimer((LoadMoreData.MinimumShowTime - interv), StopActivityIndicator);
							return;
						}
					}

					StopActivityIndicator();
				}
			}
		}
	}

}
