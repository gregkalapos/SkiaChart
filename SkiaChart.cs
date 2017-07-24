using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace SkiaChart
{
	public enum PointColor { Green, Red }

	public class SkiaChart
	{
		readonly SKCanvas canvas;
		readonly int width;
		readonly int heigh;

		struct Point
		{
			public float X;
			public float Y;

			public Point(float x, float y)
			{
				X = x;
				Y = y;
			}
		}

        public IEnumerable<Decimal> Gains
        {
            get;
            set;
        }

        public List<decimal> GainsList
        {
            get { return new List<decimal>(Gains); }
        }

        public IEnumerable<DateTime> Dates
		{
			get;
			set;
		}

		public IEnumerable<Tuple<Decimal, PointColor>> Values
		{
			get;
			set;
		}

        //TODO: very bad
        public List<DateTime> DatesList
        {
            get { return new List<DateTime>(Dates); }
        }

        public List<Tuple<Decimal,PointColor>> ValuesList
        {
			get { return new List<Tuple<Decimal, PointColor>>(Values); }
        }

		public SkiaChart(SKSurface surface, int width, int heigh)
		{
			this.heigh = heigh;
			this.width = width;
			this.canvas = surface.Canvas;
		}

		public void RenderChart()
		{
            //Console.WriteLine($"Dates size: {DatesList.Count()}");
            //Console.WriteLine($"Values size: {ValuesList.Count()}");

            //Console.WriteLine($"First Date: {DatesList.First().ToString()}");
            //Console.WriteLine($"First value: {ValuesList.First().Item1.ToString()} - {ValuesList.First().Item2.ToString()}");


			// clear the canvas / fill with white
			canvas.Clear(SKColors.White);

			// set up drawing tools
			using (var paint = new SKPaint())
			{
				paint.IsAntialias = true;
				paint.Color = new SKColor(0x2c, 0x3e, 0x50);
				paint.StrokeCap = SKStrokeCap.Round;

				var xCoordStart = new Point(0 + (width / 20), heigh - (heigh / 3));
				var xCoordEnd = new Point(width - (width / 20), heigh - (heigh / 3));

				var yCoordStart = new Point(0 + (width / 20), heigh - (heigh / 3));
				var yCoordEnd = new Point(0 + (width / 20), 0 + (heigh / 95));

                var zCoordStart = new Point(width - (width/20), xCoordStart.Y - 150);
                var zCoordEnd = new Point(width - (width / 20), xCoordStart.Y + 150);

				canvas.DrawLine(xCoordStart.X, xCoordStart.Y, xCoordEnd.X, xCoordEnd.Y, new SKPaint());
				canvas.DrawLine(yCoordStart.X, yCoordStart.Y, yCoordEnd.X, yCoordEnd.Y, new SKPaint());
                canvas.DrawLine(zCoordStart.X, zCoordStart.Y, zCoordEnd.X, zCoordEnd.Y, new SKPaint());

                //TODO: temporary workaround ;)
                canvas.DrawLine(zCoordStart.X, zCoordStart.Y-200, zCoordEnd.X, zCoordEnd.Y, new SKPaint());
				//assume: data to x and y coord are ordered 

				//map xcoord between xstart and xend;
				var xRange = xCoordEnd.X - xCoordStart.X;
                var xPlotUnit = xRange / DatesList.Count;

				//map ycoord between ystart and yend
				var yRange = yCoordStart.Y - yCoordEnd.Y;
				var yPlotUnit = yRange / (float)(Values.Select(n => n.Item1).Max() - Values.Select(n => n.Item1).Min());

                //map zcoord between zstart and zend
                var zRange = zCoordStart.Y - zCoordEnd.Y;
                var zPlotUnit = zRange / (float)(Gains.Max() - Gains.Min());

                var maxGains = GainsList.Max();
                var minGains = GainsList.Min();
				//Plot the values
				for (int i = 0; i < DatesList.Count; i++)
				{
					var xCoord = xCoordStart.X + (i * xPlotUnit);
					var yCoord = yCoordStart.Y - (yPlotUnit * (float)(ValuesList[i].Item1 - Values.Select(n => n.Item1).Min()));
					//canvas.DrawPoint(xCoord,yCoord, new SKPaint());

					var skPaint = new SKPaint();

					switch (ValuesList[i].Item2)
					{
						case PointColor.Green:
							skPaint.Color = new SKColor(0, 255, 0);
							break;
						case PointColor.Red:
							skPaint.Color = new SKColor(255, 0, 0);
							break;
						default:
							break;
					}

					canvas.DrawCircle(xCoord, yCoord, 1, skPaint);

                    var gainyCoord = zCoordEnd.Y-150 + (zPlotUnit * (float)(GainsList[i]));

                    if(GainsList[i] == maxGains || GainsList[i] == minGains )
                    {
                        canvas.DrawLine(zCoordEnd.X - 5, gainyCoord, zCoordEnd.X + 5, gainyCoord, new SKPaint());

                        var plusSign = GainsList[i] > 0 ? "+" : String.Empty;
                        canvas.DrawText(plusSign + (int)GainsList[i]+ "%", zCoordEnd.X+8, gainyCoord+5, new SKPaint());
                    }

                    canvas.DrawCircle(xCoord, gainyCoord, 1, new SKPaint());
				}

                canvas.DrawText( "0%", zCoordEnd.X + 8, zCoordEnd.Y - 153 , new SKPaint());

				//add x labels 
				var nOfXLabels = 8;
				var xLabelPlotUnit = xRange / (nOfXLabels - 1);


				for (int i = 0; i < (nOfXLabels - 1); i++)
				{
					canvas.DrawLine(xCoordStart.X + (xLabelPlotUnit * i),
									yCoordStart.Y - 5, xCoordStart.X + (xLabelPlotUnit * i), yCoordStart.Y + 5,
									new SKPaint());

					var txt = DatesList[(i) * DatesList.Count() / (nOfXLabels - 1)].ToShortDateString();
					canvas.DrawText(txt, xCoordStart.X + (xLabelPlotUnit * i) - 35,
									yCoordStart.Y + 25, new SKPaint());
				}
				canvas.DrawLine(xCoordEnd.X,
									yCoordStart.Y - 5, xCoordEnd.X, yCoordStart.Y + 5,
									new SKPaint());
				canvas.DrawText(Dates.Last().ToShortDateString(), xCoordEnd.X - 35,
								yCoordStart.Y + 25, new SKPaint());


				//add y labels
				var nOfYLabels = 8;
				var yLabelPlotUnit = yRange / (nOfYLabels - 1);

				var vv = (float)(Values.Select(n => n.Item1).Max() - Values.Select(n => n.Item1).Min());
				var vv2 = vv / (nOfYLabels - 1);
				for (int i = 0; i < (nOfYLabels - 1); i++)
				{
					canvas.DrawLine(yCoordStart.X - 5, yCoordStart.Y - (yLabelPlotUnit * i),
									yCoordStart.X + 5, yCoordStart.Y - (yLabelPlotUnit * i), new SKPaint());


					var val = Values.Select(n => n.Item1).Min() + (Decimal)i * (decimal)vv2;
					canvas.DrawText(((int)val).ToString(), yCoordStart.X - 38,
									yCoordStart.Y - (yLabelPlotUnit * i) + 5, new SKPaint());
				}

				canvas.DrawLine(yCoordStart.X - 5, yCoordEnd.Y - 1,
								   yCoordStart.X + 5, yCoordEnd.Y - 1, new SKPaint());

				canvas.DrawText(((int)Values.Select(n => n.Item1).Max()).ToString(), yCoordStart.X - 38,
								yCoordEnd.Y + 5, new SKPaint());


                //Add z. label (gains)


			}
		}
	}
}
