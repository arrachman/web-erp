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
		
		public function f_copyTransaksi(obj:Object = null, modulid:Number = 0, menuid:Number = 0):void
		{
			var vCMI:ContextMenuItem, ob:Object;
			
			if(modulid == 5 && menuid == 4){
				vCMI = new ContextMenuItem('Jadikan SO Baru');
			}else if(modulid == 3 && menuid == 4){
				vCMI = new ContextMenuItem('Jadikan TS Baru');
			}
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