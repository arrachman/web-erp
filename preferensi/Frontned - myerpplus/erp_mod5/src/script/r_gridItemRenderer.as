package script
{
	import spark.skins.spark.DefaultGridItemRenderer;

	public class r_gridItemRenderer extends DefaultGridItemRenderer
	{
		public function r_gridItemRenderer()
		{
			super();
		}
		
		public function set columnTextAlign(value:String):void
		{
			setStyle("textAlign",value);
			styleChanged("textAlign");
		}
	}
}