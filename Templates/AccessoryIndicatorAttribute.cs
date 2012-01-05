using System.Threading;
//
// AccessoryIndicatorAttribute.cs:
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
	using System.Reflection;
	using MonoTouch.Foundation;
	using MonoTouch.UIKit;
	
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, Inherited = false)]
	public class AccessoryIndicatorAttribute : CellViewTemplate
	{
		public override Type CellViewType { get { return typeof(AccessoryIndicatorCellView); } }

		public float GraceTime { get; set; }
		public float MinimumShowTime { get; set; }

		public AccessoryIndicatorAttribute(string commandMemberName): base()
		{
			CommandMemberName = commandMemberName;
		}

		public string CommandMemberName { get; set; }
		
		class AccessoryIndicatorCellView: CellView<object>, ICellContent, ISelectable, ICommandButton
		{
			private AccessoryIndicatorAttribute AccessoryIndicatorData { get { return (AccessoryIndicatorAttribute)CellViewTemplate; } }
			
			private readonly UIActivityIndicatorView _ActivityIndicator;
			private const int _TopPosition = 11;
			private const int _IndicatorSize = 20;
			private DateTime? _ShowStarted;
			private NSTimer _Timer;

			public UIView CellContentView { get; set; }
			public UIView CellBackgroundView { get; set; }
			public UIView CellSelectedBackgroundView { get; set; }

			public string CommandMemberName { get { return AccessoryIndicatorData.CommandMemberName; } set { AccessoryIndicatorData.CommandMemberName = value; } } 
			public ICommand Command { get; set; }
			public object CommandParameter { get; set; }
			public bool Enabled { get; set; }

			public AccessoryIndicatorCellView(RectangleF frame) : base(frame)
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

			public void Selected (DialogViewController controller, UITableView tableView, object item, MonoTouch.Foundation.NSIndexPath indexPath)
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
				if (Command != null)
				{
					Command.Execute(CommandParameter);
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
				_ActivityIndicator.Hidden = false;
				_ActivityIndicator.StartAnimating();
			}
	
			private void StopActivityIndicator()
			{
				_ActivityIndicator.StopAnimating();
				_ActivityIndicator.Hidden = true;
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
						if (AccessoryIndicatorData.GraceTime > 0.0)
						{
							_Timer = NSTimer.CreateScheduledTimer(AccessoryIndicatorData.GraceTime, StartActivityIndicator);
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
						if (AccessoryIndicatorData.MinimumShowTime > 0.0 && _ShowStarted.HasValue)
						{
							double interv = (DateTime.Now - _ShowStarted.Value).TotalSeconds;
							if (interv < AccessoryIndicatorData.MinimumShowTime)
							{
								_Timer = NSTimer.CreateScheduledTimer((AccessoryIndicatorData.MinimumShowTime - interv), StopActivityIndicator);
								return;
							}
						}
	
						StopActivityIndicator();
					}
				}
			}
		}
	}
}