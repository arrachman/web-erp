package MyClass
{
	import flash.events.ContextMenuEvent;
	import flash.ui.ContextMenuItem;
	
	import mx.controls.Alert;
	import mx.core.FlexGlobals;
	
	public class RejectTransaksi
	{
		public function RejectTransaksi()			
		{
			
		}
		
		public function f_RejectTrans(obj:Object = null, moduleid:Number = 0, menuid:Number = 0, sumber:String = "", paging : int = 0, filter : String = ""):void
		{
			var vCMI:ContextMenuItem, ob:Object;
			vCMI = new ContextMenuItem('Reject Transaksi');
			vCMI.addEventListener(ContextMenuEvent.MENU_ITEM_SELECT, f_Reject); 
			obj.contextMenu.customItems.push(vCMI);
			
			function f_Reject():void
			{
				if(FlexGlobals.topLevelApplication.apaBisaAkses(moduleid, menuid, 2) == true){
					FlexGlobals.topLevelApplication.f_refreshIDmodule(obj);
					var status:int = obj.dataProvider.getItemAt(obj.selectedCell.rowIndex)[sumber.toLowerCase()+"status"];
					var id:int = obj.dataProvider.getItemAt(obj.selectedCell.rowIndex)[sumber.toLowerCase()+"id"];
					if(obj.dataProviderLength > 0)
						if(FlexGlobals.topLevelApplication.apaBisaAkses(moduleid, menuid, 2) == true)
							if(status == 2){
								FlexGlobals.topLevelApplication.F_wsUpdateStatus(FlexGlobals.topLevelApplication.sptParam+'updateStatus', 'M'+moduleid+'_'+sumber+'UpdateStatus', FlexGlobals.topLevelApplication.userid, id, 6, true, int(paging), 20, filter, FlexGlobals.topLevelApplication.m.sort[FlexGlobals.topLevelApplication.module]);	
							}
							else {
								Alert.show(FlexGlobals.topLevelApplication.l('Hanya status approved yang bisa di jadikan Draft'), FlexGlobals.topLevelApplication.l('Informasi'));
							}
				}
			}
		}
	}
}