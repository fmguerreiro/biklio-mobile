using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Forms;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// Page responsible for presenting a graph plot to the user, showing the distribution per activity.
	/// </summary>
	public class DrawPlotPage : ContentPage {

		public DrawPlotPage() {
			var plot = new PlotView() {
				Model = buildPieModel(),
				Scale = 0.8
			};
			Content = plot;
		}

		public DrawPlotPage(UnitPerActivity stats, string title) {
			var plot = new PlotView() {
				Model = buildHistogram(stats, title),
				Scale = 0.8
			};
			Content = plot;
		}

		PlotModel buildHistogram(UnitPerActivity stats, string title) {
			var model = new PlotModel { Title = title };
			model.LegendBorderThickness = 0;
			model.PlotAreaBorderThickness = new OxyThickness(left: 1.0, top: 0.0, right: 0.0, bottom: 1.0);

			var xAxis = new CategoryColorAxis();
			xAxis.ActualLabels.Add(Language.Walking);
			xAxis.ActualLabels.Add(Language.Running);
			xAxis.ActualLabels.Add(Language.Cycling);
			xAxis.ActualLabels.Add(Language.Driving);
			xAxis.TickStyle = TickStyle.None; // or Outside
			xAxis.IntervalLength = 50;
			xAxis.GapWidth = 0.2;

			var yAxis = new LinearAxis();
			yAxis.Position = AxisPosition.Left;
			yAxis.TickStyle = TickStyle.None;
			yAxis.MajorGridlineStyle = LineStyle.Solid;
			yAxis.MajorGridlineColor = OxyColors.PowderBlue;
			yAxis.MajorGridlineThickness = 1.5;
			yAxis.MinorGridlineColor = OxyColors.Gray;
			yAxis.MinorGridlineStyle = LineStyle.LongDash;

			model.Axes.Add(xAxis);
			model.Axes.Add(yAxis);
			model.LegendColumnSpacing = 2.0;

			var series = new ColumnSeries();
			series.ColumnWidth = 10;
			series.FillColor = OxyColors.LightSeaGreen;
			series.LabelMargin = 2.5;

			model.Series.Add(series);
			series.Items.Add(new ColumnItem(stats.Walking));
			series.Items.Add(new ColumnItem(stats.Running));
			series.Items.Add(new ColumnItem(stats.Cycling));
			series.Items.Add(new ColumnItem(stats.Driving));

			return model;
		}

		PlotModel buildPie(UnitPerActivity stats, string title) {
			var model = new PlotModel { Title = title };
			model.Axes.Add(new CategoryAxis());
			var ps = new PieSeries {
				StrokeThickness = 2.0,
				InsideLabelPosition = 0.8,
				AngleSpan = 360,
				StartAngle = 0
			};
			ps.Slices.Add(new PieSlice(Language.Walking, stats.Walking));
			ps.Slices.Add(new PieSlice(Language.Running, stats.Running));
			ps.Slices.Add(new PieSlice(Language.Cycling, stats.Cycling));
			ps.Slices.Add(new PieSlice(Language.Driving, stats.Driving));
			model.Series.Add(ps);
			return model;
		}

		/// <summary>
		/// Examples for building charts.
		/// </summary>
		public static PlotModel buildPieModel() {
			var model = new PlotModel { Title = "World population by continent" };
			var ps = new PieSeries {
				StrokeThickness = 2.0,
				InsideLabelPosition = 0.8,
				AngleSpan = 360,
				StartAngle = 0
			};
			ps.Slices.Add(new PieSlice("Africa", 1030) { IsExploded = true });
			ps.Slices.Add(new PieSlice("Americas", 929) { IsExploded = true });
			ps.Slices.Add(new PieSlice("Asia", 4157));
			ps.Slices.Add(new PieSlice("Europe", 739) { IsExploded = true });
			ps.Slices.Add(new PieSlice("Oceania", 35) { IsExploded = true });
			model.Series.Add(ps);
			return model;
		}

		public static PlotModel buildLineSeries() {
			var plotModel = new PlotModel { Title = "OxyPlot Demo" };

			plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
			plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0 });

			var series1 = new LineSeries {
				MarkerType = MarkerType.Circle,
				MarkerSize = 4,
				MarkerStroke = OxyColors.White
			};

			series1.Points.Add(new DataPoint(0.0, 6.0));
			series1.Points.Add(new DataPoint(1.4, 2.1));
			series1.Points.Add(new DataPoint(2.0, 4.2));
			series1.Points.Add(new DataPoint(3.3, 2.3));
			series1.Points.Add(new DataPoint(4.7, 7.4));
			series1.Points.Add(new DataPoint(6.0, 6.2));
			series1.Points.Add(new DataPoint(8.9, 8.9));

			plotModel.Series.Add(series1);
			return plotModel;
		}
	}
}
