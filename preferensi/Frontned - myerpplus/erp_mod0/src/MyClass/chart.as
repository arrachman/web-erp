package MyClass
{
	import flash.external.ExternalInterface;
	
	import mx.charts.ChartItem;
	import mx.charts.chartClasses.Series;
	import mx.charts.series.ColumnSeries;
	import mx.charts.series.items.ColumnSeriesItem;
	import mx.controls.Alert;

	public class chart
	{
		public function chart()
		{
		}
		
		public function labelfunction(element:ChartItem, series:Series):String {
			var item:ColumnSeriesItem = ColumnSeriesItem(element);
			var ser:ColumnSeries = ColumnSeries(series);
//			return(item.item.Country + ":" +"" + ser.yField.toString() +":"+ item.yNumber);
			return "asdfasdf";
		}

	}
}