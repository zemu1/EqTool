﻿using EQTool.Models;
using EQTool.Services;
using EQTool.Shapes;
using EQToolShared.Map;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static EQTool.Services.MapLoad;

namespace EQTool.ViewModels
{
    public class PlayerLocationCircle
    {
        public Ellipse Ellipse;
        public ArrowLine ArrowLine;
        public Ellipse TrackingEllipse;
    }
    public class PlayerLocation : PlayerLocationCircle
    {
        public SignalrPlayer Player;
    }

    public class MapViewModel : INotifyPropertyChanged
    {
        private readonly MapLoad mapLoad;
        private readonly ActivePlayer activePlayer;
        private readonly LoggingService loggingService;
        private MatrixTransform Transform = new MatrixTransform();
        private Point _initialMousePosition;
        private Point _mouseuppoint;
        private Point3D MapOffset = new Point3D(0, 0, 0);
        private bool MapLoading = false;
        private PlayerLocationCircle PlayerLocation;
        private List<PlayerLocation> Players = new List<PlayerLocation>();
        private Canvas Canvas;
        private float CurrentScaling = 1.0f;
        private readonly float Zoomfactor = 1.1f;
        private bool _dragging;
        private UIElement _selectedElement;
        private Vector _draggingDelta;
        private bool TimerOpen = false;
        private readonly TimersService timersService;

        public string MouseLocation => $"   {LastMouselocation.Y:0.##}, {LastMouselocation.X:0.##}";
        public double ZoneLabelFontSize => MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 14, 170);
        public double OtherLabelFontSize => MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 6, 110);
        public double SmallFontSize => MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 7, 50);

        public AABB AABB = new AABB();

        public MapViewModel(MapLoad mapLoad, ActivePlayer activePlayer, LoggingService loggingService, TimersService timersService)
        {
            this.timersService = timersService;
            this.mapLoad = mapLoad;
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
        }

        private TimeSpan _TimerValue = TimeSpan.FromMinutes(72);
        public TimeSpan TimerValue
        {
            get => _TimerValue;
            set
            {
                _TimerValue = value;
                OnPropertyChanged();
            }
        }

        private Point3D _Lastlocation = new Point3D(0, 0, 0);
        public Point3D Lastlocation
        {
            get => _Lastlocation;
            set
            {
                _Lastlocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Title => _ZoneName + "  v" + App.Version + $"   {Lastlocation.X:0.##}, {Lastlocation.Y:0.##}, {Lastlocation.Z:0.##}";

        private string _ZoneName = string.Empty;

        public string ZoneName
        {
            get => _ZoneName;
            set
            {
                _ZoneName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        private Point _LastMouselocation = new Point(0, 0);

        private Point LastMouselocation
        {
            get => _LastMouselocation;
            set
            {
                _LastMouselocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MouseLocation));
            }
        }

        private void Reset()
        {
            Transform = new MatrixTransform();
            CurrentScaling = 1.0f;
            Canvas?.Children?.Clear();
            Players = new List<PlayerLocation>();
        }

        public bool LoadMap(string zone, Canvas canvas)
        {
            if (MapLoading)
            {
                return false;
            }
            zone = ZoneParser.TranslateToMapName(zone);
            if (string.IsNullOrWhiteSpace(zone))
            {
                zone = "freportw";
            }
            if (zone == ZoneName)
            {
                return false;
            }
            Canvas = canvas;
            MapLoading = true;
            var stop = new Stopwatch();
            stop.Start();
            try
            {
                var map = mapLoad.Load(zone);
                if (map.Labels.Any() || map.Lines.Any())
                {
                    Reset();
                    AABB = map.AABB;
                    ZoneName = zone;
                    Debug.WriteLine($"Loading: {zone}");
                    MapOffset = map.Offset;
                    var linethickness = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 800, 35000, 2, 40);

                    foreach (var group in map.Lines)
                    {
                        var c = EQMapColor.GetThemedColors(group.Color);
                        group.ThemeColors = c;
                        var colorstuff = new SolidColorBrush(c.DarkColor);
                        var l = new Line
                        {
                            Tag = group,
                            X1 = group.Points[0].X,
                            Y1 = group.Points[0].Y,
                            X2 = group.Points[1].X,
                            Y2 = group.Points[1].Y,
                            StrokeThickness = linethickness,
                            Stroke = colorstuff,
                            RenderTransform = Transform
                        };
                        _ = canvas.Children.Add(l);
                    }

                    Debug.WriteLine($"Labels: {map.Labels.Count}");
                    var locationdotsize = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 20, 150);
                    var locationthickness = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 5, 20);
                    foreach (var item in map.Labels)
                    {
                        var text = new TextBlock
                        {
                            Tag = item,
                            Text = item.label.Replace('_', ' '),
                            FontSize = item.LabelSize == LabelSize.Large ? ZoneLabelFontSize : OtherLabelFontSize,
                            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255)),
                            RenderTransform = Transform
                        };
                        var circle = new Ellipse()
                        {
                            Tag = item,
                            Width = locationdotsize,
                            Height = locationdotsize,
                            Stroke = Brushes.Red,
                            StrokeThickness = locationthickness
                        };
                        _ = canvas.Children.Add(circle);
                        _ = canvas.Children.Add(text);
                        Canvas.SetLeft(text, item.Point.X);
                        Canvas.SetTop(text, item.Point.Y);
                        Canvas.SetLeft(circle, item.Point.X);
                        Canvas.SetTop(circle, item.Point.Y);
                    }

                    var playerlocsize = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 40, 1750);
                    var playerstrokthickness = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 3, 40);
                    var trackingdistance = this.activePlayer?.Player?.TrackingDistance;
                    PlayerLocation = new PlayerLocationCircle
                    {
                        Ellipse = new Ellipse()
                        {
                            Height = playerlocsize / 4,
                            Width = playerlocsize / 4,
                            Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                            StrokeThickness = playerstrokthickness,
                            RenderTransform = new RotateTransform()
                        },
                        ArrowLine = new ArrowLine()
                        {
                            Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                            StrokeThickness = playerstrokthickness,
                            X1 = 0,
                            Y1 = 0,
                            X2 = 0,
                            Y2 = playerlocsize,
                            ArrowLength = playerlocsize / 4,
                            ArrowEnds = ArrowEnds.End,
                            RotateTransform = new RotateTransform()
                        },
                        TrackingEllipse = new Ellipse()
                        {
                            Height = trackingdistance ?? 100,
                            Width = trackingdistance ?? 100,
                            Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                            StrokeThickness = playerstrokthickness,
                            RenderTransform = new RotateTransform(),
                            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(5, 61, 235, 52)),
                        }
                    };

                    _ = canvas.Children.Add(PlayerLocation.Ellipse);
                    Canvas.SetLeft(PlayerLocation.Ellipse, AABB.Center.X + PlayerLocation.Ellipse.Height + (PlayerLocation.Ellipse.Height / 2));
                    Canvas.SetTop(PlayerLocation.Ellipse, AABB.Center.Y + PlayerLocation.Ellipse.Height + (PlayerLocation.Ellipse.Height / 2));

                    _ = canvas.Children.Add(PlayerLocation.ArrowLine);
                    Canvas.SetLeft(PlayerLocation.ArrowLine, AABB.Center.X + (playerlocsize / 2));
                    Canvas.SetTop(PlayerLocation.ArrowLine, AABB.Center.Y + (playerlocsize / 2));
                    if (!trackingdistance.HasValue)
                    {
                        PlayerLocation.TrackingEllipse.Visibility = Visibility.Hidden;
                    }
                    _ = canvas.Children.Add(PlayerLocation.TrackingEllipse);
                    Canvas.SetLeft(PlayerLocation.TrackingEllipse, AABB.Center.X + PlayerLocation.TrackingEllipse.Height + (PlayerLocation.TrackingEllipse.Height / 2));
                    Canvas.SetTop(PlayerLocation.TrackingEllipse, AABB.Center.Y + PlayerLocation.TrackingEllipse.Height + (PlayerLocation.TrackingEllipse.Height / 2));

                    var widgets = timersService.LoadTimersForZone(ZoneName);
                    foreach (var mw in widgets)
                    {
                        _ = canvas.Children.Add(mw);
                        Canvas.SetLeft(mw, -mw.TimerInfo.Location.X - MapOffset.X);
                        Canvas.SetTop(mw, -mw.TimerInfo.Location.Y - MapOffset.Y);
                        mw.RenderTransform = new RotateTransform();
                    }
                    return true;
                }

                return false;
            }
            finally
            {
                stop.Stop();
                Debug.WriteLine($"Time to load {zone} - {stop.ElapsedMilliseconds}ms");
                MapLoading = false;
            }
        }

        public bool LoadDefaultMap(Canvas canvas)
        {
            _ = activePlayer.Update();
            var z = ZoneParser.TranslateToMapName(activePlayer.Player?.Zone);
            if (string.IsNullOrWhiteSpace(z))
            {
                z = "freportw";
            }
            return LoadMap(z, canvas);
        }

        private static double GetAngleBetweenPoints(Point3D pt1, Point3D pt2)
        {
            var dx = pt2.X - pt1.X;
            var dy = pt2.Y - pt1.Y;
            var deg = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (deg < 0) { deg += 360; }

            return deg;
        }

        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }

        private int failedzonelogcounter = 0;

        public void UpdateLocation(Point3D value1)
        {
            if (MapLoading || PlayerLocation?.ArrowLine == null || Canvas == null || string.IsNullOrWhiteSpace(ZoneName))
            {
                return;
            }

            if (!EQToolShared.Map.ZoneParser.ZoneInfoMap.TryGetValue(ZoneName, out var zoneinfo))
            {
                if (failedzonelogcounter == 0 || failedzonelogcounter++ % 20 == 0)
                {
                    loggingService.Log($"Zone {ZoneName} Not found.", App.EventType.Error);
                }
            }

            OnPropertyChanged(nameof(Title));
            var newdir = new Point3D(value1.X, value1.Y, 0) - new Point3D(Lastlocation.X, Lastlocation.Y, 0);
            newdir.Normalize();
            var angle = GetAngleBetweenPoints(new Point3D(value1.X, value1.Y, 0), new Point3D(Lastlocation.X, Lastlocation.Y, 0)) * -1;
            Lastlocation = value1;
            PlayerLocation.ArrowLine.RotateTransform = new RotateTransform(angle);
            Canvas.SetLeft(PlayerLocation.ArrowLine, -(value1.Y + MapOffset.X) * CurrentScaling);
            Canvas.SetTop(PlayerLocation.ArrowLine, -(value1.X + MapOffset.Y) * CurrentScaling);
            var heighdiv2 = PlayerLocation.Ellipse.Height / 2 / CurrentScaling;
            Canvas.SetLeft(PlayerLocation.Ellipse, -(value1.Y + MapOffset.X + heighdiv2) * CurrentScaling);
            Canvas.SetTop(PlayerLocation.Ellipse, -(value1.X + MapOffset.Y + heighdiv2) * CurrentScaling);
            var trackingdistance = this.activePlayer?.Player?.TrackingDistance;
            if (!trackingdistance.HasValue)
            {
                PlayerLocation.TrackingEllipse.Visibility = Visibility.Hidden;
            }
            else
            {
                PlayerLocation.TrackingEllipse.Visibility = Visibility.Visible;
                PlayerLocation.TrackingEllipse.Height = trackingdistance.Value;
                PlayerLocation.TrackingEllipse.Width = trackingdistance.Value;
            }
            heighdiv2 = PlayerLocation.TrackingEllipse.Height / 2;
            Canvas.SetLeft(PlayerLocation.TrackingEllipse, -(value1.Y + MapOffset.X + heighdiv2) * CurrentScaling);
            Canvas.SetTop(PlayerLocation.TrackingEllipse, -(value1.X + MapOffset.Y + heighdiv2) * CurrentScaling);

            var transform = new MatrixTransform();
            var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
            transform.Matrix = PlayerLocation.ArrowLine.RotateTransform.Value * translation.Value;
            PlayerLocation.ArrowLine.RenderTransform = transform;
            var transform2 = new MatrixTransform();
            _ = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
            transform2.Matrix = translation.Value;
            PlayerLocation.Ellipse.RenderTransform = transform2;

            if (!zoneinfo.ShowAllMapLevels && Canvas.Children.Count > 0)
            {
                var lastloc = new Point3D(-(value1.Y + MapOffset.X), -(value1.X + MapOffset.Y), Lastlocation.Z);
                _ = zoneinfo.ZoneLevelHeight * 2;
                foreach (var child in Canvas.Children)
                {
                    if (child is Line a)
                    {
                        var m = a.Tag as MapLine;
                        var shortestdistance = Math.Abs(m.Points[0].Z - lastloc.Z);
                        shortestdistance = Math.Min(Math.Abs(m.Points[1].Z - lastloc.Z), shortestdistance);
                        MapOpacityHelper.AdjustOpacity(shortestdistance, a, zoneinfo, lastloc);
                    }
                    else if (child is TextBlock t)
                    {
                        MapOpacityHelper.AdjustOpacity(t, zoneinfo, lastloc);
                    }
                    else if (child is Ellipse e)
                    {
                        if (e != PlayerLocation.Ellipse && PlayerLocation.TrackingEllipse != e)
                        {
                            var m = e.Tag as MapLabel;
                            if (m != null)
                            {
                                var shortestdistance = Math.Abs(m.Point.Z - lastloc.Z);
                                MapOpacityHelper.AdjustOpacity(shortestdistance, e, zoneinfo, lastloc);
                            }
                        }
                    }
                }
            }
        }

        public void MouseMove(Point mousePosition)
        {
            mousePosition = Transform.Inverse.Transform(mousePosition);
            mousePosition.X += MapOffset.X;
            mousePosition.Y += MapOffset.Y;
            mousePosition.X *= -1;
            mousePosition.Y *= -1;
            LastMouselocation = mousePosition;
        }

        public void UpdateTimerWidgest()
        {
            var removewidgets = new List<MapWidget>();
            foreach (var item in Canvas.Children)
            {
                if (item is MapWidget m)
                {
                    if (m.Update() <= -60 * 4)
                    {
                        removewidgets.Add(m);
                    }
                }
            }

            foreach (var item in removewidgets)
            {
                timersService.RemoveTimer(item.TimerInfo);
                Canvas.Children.Remove(item);
            }

            var playerstoremove = new List<PlayerLocation>();
            foreach (var item in Players)
            {
                if ((DateTime.UtcNow - item.Player.TimeStamp).TotalMinutes > 4)
                {
                    playerstoremove.Add(item);
                }
            }
            foreach (var item in playerstoremove)
            {
                Players.Remove(item);
                Canvas.Children.Remove(item.Ellipse);
            }
        }

        public void MoveToPlayerLocation(MapWidget mw)
        {
            if (PlayerLocation == null)
            {
                return;
            }
            mw.TimerInfo.Location = new Point(Lastlocation.Y, Lastlocation.X);
            Canvas.SetLeft(mw, Canvas.GetLeft(PlayerLocation.ArrowLine));
            Canvas.SetTop(mw, Canvas.GetTop(PlayerLocation.ArrowLine));
            mw.RenderTransform = Transform;
        }

        private int deathcounter = 1;
        public MapWidget AddTimer(TimeSpan timer, string title, bool autoIncrementDuplicateNames)
        {
            var mousePosition = Transform.Inverse.Transform(_mouseuppoint);
            mousePosition.X += MapOffset.X;
            mousePosition.Y += MapOffset.Y;
            mousePosition.X *= -1;
            mousePosition.Y *= -1;
            if (autoIncrementDuplicateNames && timersService.TimerExists(ZoneName, title))
            {
                deathcounter = ++deathcounter > 999 ? 1 : deathcounter;
                title += "_" + deathcounter;
            }
            var mw = timersService.AddTimer(new TimerInfo
            {
                Duration = timer,
                Name = title,
                ZoneName = ZoneName,
                Fontsize = SmallFontSize,
                StartTime = DateTime.Now,
                Location = mousePosition
            });
            _ = Canvas.Children.Add(mw);
            Canvas.SetTop(mw, _mouseuppoint.Y - Transform.Value.OffsetY);
            Canvas.SetLeft(mw, _mouseuppoint.X - Transform.Value.OffsetX);
            mw.RenderTransform = Transform;
            return mw;
        }

        public TimerInfo DeleteSelectedTimer()
        {
            if (_selectedElement is MapWidget w)
            {
                timersService.RemoveTimer(w.TimerInfo);
                Canvas.Children.Remove(w);
                _dragging = false;
                _selectedElement = null;
                return w.TimerInfo;
            }
            return null;
        }

        public void DeleteSelectedTimerByName(string name)
        {
            var timer = timersService.RemoveTimer(name);
            if (timer != null)
            {
                MapWidget wremove = null;
                foreach (var item in Canvas.Children)
                {
                    if (item is MapWidget w && w.TimerInfo.Name == name)
                    {
                        wremove = w;
                        break;
                    }
                }

                if (wremove != null)
                {
                    Canvas.Children.Remove(wremove);
                }
            }
        }

        public void PanAndZoomCanvas_MouseDown(Point mousePostion, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _selectedElement = e.Source is MapWidget ? (UIElement)e.Source : null;
            }
            if (TimerOpen)
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.Source is MapWidget)
                {
                    _selectedElement = (UIElement)e.Source;
                    var x = Canvas.GetLeft(_selectedElement);
                    var y = Canvas.GetTop(_selectedElement);
                    var elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePostion;
                    _dragging = true;
                }
                else
                {
                    _dragging = false;
                    _selectedElement = null;
                }
            }

            if (!_dragging && e.ChangedButton == MouseButton.Left)
            {
                _initialMousePosition = Transform.Inverse.Transform(mousePostion);
            }
        }

        public void PanAndZoomCanvas_MouseUp(Point mousePostion)
        {
            _mouseuppoint = mousePostion;
            _dragging = false;
        }

        public void MoveMap(int x, int y)
        {
            var translate = new TranslateTransform(x, y);
            Transform.Matrix = translate.Value * Transform.Matrix;
            foreach (UIElement child in Canvas.Children)
            {
                if (child is ArrowLine c)
                {
                    var transform = new MatrixTransform();
                    var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                    transform.Matrix = c.RotateTransform.Value * translation.Value;
                    c.RenderTransform = transform;
                }
                else
                {
                    child.RenderTransform = Transform;
                }
            }
        }

        public void PanAndZoomCanvas_MouseMove(Point mousePostion, MouseEventArgs e)
        {
            if (TimerOpen)
            {
                return;
            }

            if (!_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = Transform.Inverse.Transform(mousePostion);
                var delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                Transform.Matrix = translate.Value * Transform.Matrix;
                foreach (UIElement child in Canvas.Children)
                {
                    if (child is ArrowLine c)
                    {
                        var transform = new MatrixTransform();
                        var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                        transform.Matrix = c.RotateTransform.Value * translation.Value;
                        c.RenderTransform = transform;
                    }
                    else if (child is Ellipse el && PlayerLocation.Ellipse == el)
                    {
                        var transform = new MatrixTransform();
                        var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                        transform.Matrix = translation.Value;
                        el.RenderTransform = transform;
                    }
                    else
                    {
                        child.RenderTransform = Transform;
                    }
                }
            }

            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, mousePostion.X + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement, mousePostion.Y + _draggingDelta.Y);
                    if (_selectedElement is MapWidget m)
                    {
                        var point = new Point(Canvas.GetLeft(_selectedElement), Canvas.GetTop(_selectedElement));
                        Debug.WriteLine($"2 {point}");
                        point = _selectedElement.RenderTransform.Inverse.Transform(point);
                        Debug.WriteLine($"3 {point}");
                        m.TimerInfo.Location = _selectedElement.RenderTransform.Inverse.Transform(point);
                    }
                }
            }
        }

        public void PanAndZoomCanvas_MouseWheel(Point mousePostion, int delta)
        {
            if (TimerOpen || _dragging)
            {
                return;
            }

            var scaleFactor = Zoomfactor;
            if (delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
                if (CurrentScaling == 1.0f)
                {
                    return;
                }
            }

            if (CurrentScaling * scaleFactor > 40)
            {
                return;
            }
            if (CurrentScaling * scaleFactor < 1 && CurrentScaling != 1.0f)
            {
                scaleFactor = 1.0f / CurrentScaling;
            }

            var scaleMatrix = Transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            Transform.Matrix = scaleMatrix;
            CurrentScaling *= scaleFactor;
            var currentlabelscaling = (CurrentScaling / 40 * -1) + 1;
            foreach (UIElement child in Canvas.Children)
            {
                var x = Canvas.GetLeft(child);
                var y = Canvas.GetTop(child);

                var sx = x * scaleFactor;
                var sy = y * scaleFactor;


                if (child is ArrowLine c)
                {
                    Canvas.SetLeft(child, sx);
                    Canvas.SetTop(child, sy);
                    var transform = new MatrixTransform();
                    var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                    transform.Matrix = c.RotateTransform.Value * translation.Value;
                    c.RenderTransform = transform;
                }
                else if (child is Ellipse el)
                {
                    if (PlayerLocation.Ellipse == el)
                    {
                        var heighdiv2 = el.Height / 2 / CurrentScaling;
                        Canvas.SetLeft(el, -(Lastlocation.Y + MapOffset.X + heighdiv2) * CurrentScaling);
                        Canvas.SetTop(el, -(Lastlocation.X + MapOffset.Y + heighdiv2) * CurrentScaling);
                    }
                    else if (PlayerLocation.TrackingEllipse == el)
                    {
                        var heighdiv2 = el.Height / 2;
                        Canvas.SetLeft(el, -(Lastlocation.Y + MapOffset.X + heighdiv2) * CurrentScaling);
                        Canvas.SetTop(el, -(Lastlocation.X + MapOffset.Y + heighdiv2) * CurrentScaling);
                        child.RenderTransform = Transform;
                        continue;
                    }
                    else
                    {
                        Canvas.SetLeft(child, sx);
                        Canvas.SetTop(child, sy);
                    }

                    var transform = new MatrixTransform();
                    var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                    transform.Matrix = translation.Value;
                    el.RenderTransform = transform;
                }
                else if (child is TextBlock t)
                {
                    Canvas.SetLeft(child, sx);
                    Canvas.SetTop(child, sy);
                    var textdata = t.Tag as MapLabel;
                    if (textdata.LabelSize == LabelSize.Large)
                    {
                        var largescaling = ZoneLabelFontSize;
                        largescaling *= currentlabelscaling;
                        largescaling = (int)Clamp(largescaling, 5, 200);
                        if (t.FontSize != largescaling)
                        {
                            t.FontSize = largescaling;
                        }
                    }
                    else
                    {
                        var smallscaling = OtherLabelFontSize;
                        smallscaling *= currentlabelscaling;
                        smallscaling = (int)Clamp(smallscaling, 5, 100);
                        if (t.FontSize != smallscaling)
                        {
                            t.FontSize = smallscaling;
                        }
                    }
                }
                else
                {
                    Canvas.SetLeft(child, sx);
                    Canvas.SetTop(child, sy);
                    child.RenderTransform = Transform;
                }
            }
        }

        public void TimerMenu_Closed()
        {
            TimerOpen = false;
        }

        public void TimerMenu_Opened()
        {
            TimerOpen = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void PlayerLocationEvent(SignalrPlayer e)
        {
            var p = this.Players.FirstOrDefault(a => a.Player.Name == e.Name);
            if (p == null)
            {
                var playerlocsize = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 40, 1750);
                var playerstrokthickness = MathHelper.ChangeRange(Math.Max(AABB.MaxWidth, AABB.MaxHeight), 500, 35000, 3, 40);
                var trackingdistance = this.activePlayer?.Player?.TrackingDistance;
                var player = new PlayerLocation
                {
                    Ellipse = new Ellipse()
                    {
                        Height = playerlocsize / 4,
                        Width = playerlocsize / 4,
                        Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                        StrokeThickness = playerstrokthickness,
                        RenderTransform = new RotateTransform()
                    },
                    ArrowLine = new ArrowLine()
                    {
                        Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                        StrokeThickness = playerstrokthickness,
                        X1 = 0,
                        Y1 = 0,
                        X2 = 0,
                        Y2 = playerlocsize,
                        ArrowLength = playerlocsize / 4,
                        ArrowEnds = ArrowEnds.End,
                        RotateTransform = new RotateTransform()
                    },
                    TrackingEllipse = new Ellipse()
                    {
                        Height = trackingdistance ?? 100,
                        Width = trackingdistance ?? 100,
                        Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(61, 235, 52)),
                        StrokeThickness = playerstrokthickness,
                        RenderTransform = new RotateTransform(),
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(5, 61, 235, 52)),
                        Visibility = Visibility.Hidden
                    },
                    Player = e
                };

                _ = Canvas.Children.Add(player.Ellipse);
                Canvas.SetLeft(player.Ellipse, AABB.Center.X + player.Ellipse.Height + (player.Ellipse.Height / 2));
                Canvas.SetTop(player.Ellipse, AABB.Center.Y + player.Ellipse.Height + (player.Ellipse.Height / 2));
                this.Players.Add(player);
            }
            else
            {
                var newdir = new Point3D(e.X, e.Y, 0) - new Point3D(p.Player.X, p.Player.Y, 0);
                newdir.Normalize();
                var angle = GetAngleBetweenPoints(new Point3D(e.X, e.Y, 0), new Point3D(p.Player.X, p.Player.Y, 0)) * -1;
                p.Player = e;
                var heighdiv2 = p.Ellipse.Height / 2 / CurrentScaling;
                Canvas.SetLeft(p.Ellipse, -(e.Y + MapOffset.X + heighdiv2) * CurrentScaling);
                Canvas.SetTop(p.Ellipse, -(e.X + MapOffset.Y + heighdiv2) * CurrentScaling);

                var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                var transform2 = new MatrixTransform();
                _ = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                transform2.Matrix = translation.Value;
                p.Ellipse.RenderTransform = transform2;
            }
        }

        public void PlayerDisconnected(SignalrPlayer e)
        {
            var p = this.Players.FirstOrDefault(a => a.Player.Name == e.Name);
            if (p != null)
            {
                Canvas.Children.Remove(p.Ellipse);
            }
        }
    }
}
