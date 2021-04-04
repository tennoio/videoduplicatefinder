using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace VideoDuplicateFinderWindows
{
    static class Utils
    {
        /// <summary>
        /// Helper function to retreive objects of a specific kind from visual tree
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<T> GetVisualTreeObjects<T>(this DependencyObject obj) where T : DependencyObject
        {
            var objects = new List<T>();
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T requestedType)
                    objects.Add(requestedType);
                objects.AddRange(child.GetVisualTreeObjects<T>());
            }
            return objects;
        }
        /// <summary>
        /// Toggle expander within a listview
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="expand"></param>
        public static void ToggleExpander(this System.Windows.Controls.ListView listView, bool expand)
        {
            foreach (var e in GetVisualTreeObjects<System.Windows.Controls.Expander>(listView))
                e.IsExpanded = expand;
        }

        private static BitmapImage BitmapToBitmapImage(Image src)
        {
			using var memory = new MemoryStream();
			var bmp = new Bitmap(src);
			bmp.Save(memory, ImageFormat.Jpeg);
			memory.Position = 0;

			var bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memory;
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.EndInit();
			bitmapImage.Freeze();
			return bitmapImage;
		}
        public static BitmapImage? JoinImages(List<Image> pImgList)
        {
			if(pImgList.Count == 0) return null;
            if (pImgList.Count == 1)
                return BitmapToBitmapImage(pImgList[0]);
            var height = pImgList[0].Height;
            var width = 0;
            for (var i = 0; i <= pImgList.Count - 1; i++)
                width += pImgList[i].Width;
            var img = new Bitmap(width, height);
            using (var g = Graphics.FromImage(img))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                var tmpwidth = 0;
                for (var i = 0; i <= pImgList.Count - 1; i++)
                {
                    g.DrawImage(pImgList[i], tmpwidth, 0);
                    tmpwidth += pImgList[i].Width;
                }
                g.Save();
            }
            pImgList.Clear();

            return BitmapToBitmapImage(img);
        }

		// many thanks to Rick Strahl at https://stackoverflow.com/a/44876143
		public class DebounceDispatcher {
			private DispatcherTimer timer;
			private DateTime timerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

			public void Debounce(int interval, Action<object> action,
				object param = null,
				DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
				Dispatcher disp = null) {
				// kill pending timer and pending ticks
				timer?.Stop();
				timer = null;

				if (disp == null)
					disp = Dispatcher.CurrentDispatcher;

				// timer is recreated for each event and effectively
				// resets the timeout. Action only fires after timeout has fully
				// elapsed without other events firing in between
				timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
				{
					if (timer == null)
						return;

					timer?.Stop();
					timer = null;
					action.Invoke(param);
				}, disp);

				timer.Start();
			}
		}
	}
}
