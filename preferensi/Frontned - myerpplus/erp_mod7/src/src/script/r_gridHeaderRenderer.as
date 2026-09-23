package script
{
	import spark.skins.spark.DefaultGridHeaderRenderer;
	
	public class r_gridHeaderRenderer extends DefaultGridHeaderRenderer
	{
		public function r_gridHeaderRenderer()
		{
			super();
		}
		
		public function set headerTextAlign(value:String):void
		{
			labelDisplay.setStyle("textAlign",value);
			labelDisplay.styleChanged("textAlign");
		}

	}
}