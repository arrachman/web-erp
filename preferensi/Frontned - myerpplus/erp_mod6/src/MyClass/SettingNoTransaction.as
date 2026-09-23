package MyClass
{
	import mx.controls.Alert;
	import mx.core.FlexGlobals;
	
	import spark.components.CheckBox;
	import spark.components.TextInput;

	public class SettingNoTransaction
	{
		
		public var vNotransaksi:int = 0, vDefNotransaksi:int = 0;
		
		public function SettingNoTransaction(module:int = -1, sumber:String = "")
		{
			if(module >= 0 && sumber.length > 0)
			{
				vNotransaksi = int(FlexGlobals.topLevelApplication.getSetting(module, 'options', sumber + 'AutoNoTransaction'));
				vDefNotransaksi = int(FlexGlobals.topLevelApplication.getSetting(module, 'options', sumber + 'DefaultAutoNoTransaction'));
			}
			else
			{
				vNotransaksi = int(FlexGlobals.topLevelApplication.getSetting(0, 'accounting', 'AutoNoTransaction'));
				vDefNotransaksi = int(FlexGlobals.topLevelApplication.getSetting(0, 'accounting', 'DefaultAutoNoTransaction'));
			}
		}
		
		public function F_KondisiAwalNoTransaksi(txtnotransaksi:TextInput, chkauto:CheckBox):void
		{
			switch(vNotransaksi)
			{
				case 0:
					txtnotransaksi.text = "";
					txtnotransaksi.enabled = true;
					chkauto.selected = false; 
					chkauto.enabled = false; 
					break;
				case 1:
					txtnotransaksi.text = "Auto";
					txtnotransaksi.enabled = false;
					chkauto.selected = true; 
					chkauto.enabled = false;
					break;
				case 2:
					if(vDefNotransaksi == 1)
					{
						txtnotransaksi.text = "Auto";
						txtnotransaksi.enabled = false;
						chkauto.selected = true;
						chkauto.enabled = true;
					}
					else
					{
						txtnotransaksi.text = "";
						txtnotransaksi.enabled = true;
						chkauto.selected = false; 
						chkauto.enabled = true;
					}
					break;
			}
		}
		
		public function F_ClickCheckboxAuto(txtnotransaksi:TextInput, chkauto:CheckBox, getNotransaksi:String=''):void
		{
			// Jika tampil detail transaksi data maka auto disabled
			switch(vNotransaksi)
			{
				case 0:
					txtnotransaksi.text = "";
					txtnotransaksi.enabled = true;
					chkauto.selected = false; 
					chkauto.enabled = false;
					break;
				case 1:
					txtnotransaksi.text = "Auto";
					txtnotransaksi.enabled = false;
					chkauto.selected = true; 
					chkauto.enabled = false;
					break;
				case 2:
					if(chkauto.selected)
					{
						if(vNotransaksi == 2 && vDefNotransaksi == 0){
							txtnotransaksi.text = 'Auto';
							txtnotransaksi.enabled = false;
						}else if(vNotransaksi == 2 && vDefNotransaksi == 1){
							txtnotransaksi.text = 'Auto';
							txtnotransaksi.enabled = false;
						}
					}
					else
					{
						if(vNotransaksi == 2 && vDefNotransaksi == 0){
							txtnotransaksi.text = '';
							txtnotransaksi.enabled = true;
							txtnotransaksi.setFocus();
						}else if(vNotransaksi == 2 && vDefNotransaksi == 1){
							txtnotransaksi.text = '';
							txtnotransaksi.enabled = true;
							txtnotransaksi.setFocus();
						}
					}
					break;
			}
		}
	}
}