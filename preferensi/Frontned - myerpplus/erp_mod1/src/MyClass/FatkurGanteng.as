package MyClass
{
	import flash.events.ContextMenuEvent;
	import flash.ui.ContextMenuItem;
	
	import mx.core.FlexGlobals;
	
	import org.flexunit.runners.ParentRunner;
	
	import spark.components.DataGrid;

	public class FatkurGanteng
	{
		public function FatkurGanteng()
		{
			
		}
		
		public function f_copyTransaksi(obj:Object = null):void
		{
			var vCMI:ContextMenuItem, ob:Object;
			
			vCMI = new ContextMenuItem('Copy Transaksi')
			vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_copyTransaksi); 
			obj.contextMenu.customItems.push(vCMI);
			ob = FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.module].child;
			
			function f_copyTransaksi():void
			{
				FlexGlobals.topLevelApplication.f_refreshIDmodule(obj);
				FlexGlobals.topLevelApplication.openForm(ob.a[0], ob.a[1], obj.dataProvider.getItemAt(obj.selectedCell.rowIndex)[ob.a[3].toLowerCase()+'id'], ["copyTransaksi"]);
			}
		}
	}
}