package MyClass
{
	import mx.controls.Alert;
	
	import spark.components.DataGrid;
	import spark.modules.Module;
	
	public class KelasKu
	{
		protected var i:int;
		
		public function KelasKu()
		{
		}
		
		public function KondisiEditGrid(dg:DataGrid, ar:Array):Boolean
		{
			try
			{
				if(dg.selectedCell == null)
					return false;
				return Boolean(int(ar[dg.selectedCell.columnIndex]));
			} 
			catch(error:Error) 
			{
				return false;
			}
			return false;
		}
		
		public function getDayCount(year:int, month:int):int{
			var d:Date = new Date(year, month, 0);
			return d.getDate();
		}
	}
	
}

