package MyClass
{
	import mx.controls.Alert;
	import mx.core.FlexGlobals;
	
	import spark.components.CheckBox;
	import spark.components.TextInput;

	public class SettingNoTransaction
	{
		
		public function SettingNoTransaction()
		{
		}
		
		public var vNotransaksi:String = FlexGlobals.topLevelApplication.getSetting(0, 'accounting', 'AutoNoTransaction');
		public var vDefNotransaksi:String = FlexGlobals.topLevelApplication.getSetting(0, 'accounting', 'DefaultAutoNoTransaction');
		public function F_KondisiAwalNoTransaksi(txtnotransaksi:TextInput, chkauto:CheckBox):void{
			if(vNotransaksi == '0'){
				txtnotransaksi.text = "";
				txtnotransaksi.enabled = true;
				chkauto.selected = false; 
				chkauto.enabled = false;
			}else if(vNotransaksi == '1'){
				txtnotransaksi.text = "Auto";
				txtnotransaksi.enabled = false;
				chkauto.selected = true; 
				chkauto.enabled = false;
			}else if(vNotransaksi == '2'){
				if(vDefNotransaksi == '1'){
					txtnotransaksi.text = "Auto";
					txtnotransaksi.enabled = false;
					chkauto.selected = true;
					chkauto.enabled = true;
				}else{
					txtnotransaksi.text = "";
					txtnotransaksi.enabled = true;
					chkauto.selected = false; 
					chkauto.enabled = true;
				}
			}
		}
		
		public function F_ClickCheckboxAuto(txtnotransaksi:TextInput, chkauto:CheckBox, getNotransaksi:String=''):void{
			if(FlexGlobals.topLevelApplication.FormFilter != ''){
//				if(vNotransaksi == '0'){
//					txtnotransaksi.text = getNotransaksi;
//					txtnotransaksi.enabled = true;
//					chkauto.selected = false; 
//					chkauto.enabled = false;
//				}else if(vNotransaksi == '1'){
//					txtnotransaksi.text = getNotransaksi;
//					txtnotransaksi.enabled = false;
//					chkauto.selected = true; 
//					chkauto.enabled = false;
//				}else if(vNotransaksi == '2'){
					chkauto.enabled = false;
					if(chkauto.selected == true){
						if(vNotransaksi == '2' && vDefNotransaksi == '0'){
							txtnotransaksi.text = getNotransaksi;
							txtnotransaksi.enabled = false;
						}else if(vNotransaksi == '2' && vDefNotransaksi == '1'){
							txtnotransaksi.text = getNotransaksi;
							txtnotransaksi.enabled = false;
						}
					}else{
						if(vNotransaksi == '2' && vDefNotransaksi == '0'){
							txtnotransaksi.text = getNotransaksi;
							txtnotransaksi.enabled = true;
							txtnotransaksi.setFocus();
						}else if(vNotransaksi == '2' && vDefNotransaksi == '1'){
							txtnotransaksi.text = getNotransaksi;
							txtnotransaksi.enabled = true;
							txtnotransaksi.setFocus();
						}
					}
//				}
			}else{
				if(vNotransaksi == '0'){
					txtnotransaksi.text = "";
					txtnotransaksi.enabled = true;
					chkauto.selected = false; 
					chkauto.enabled = false;
				}else if(vNotransaksi == '1'){
					txtnotransaksi.text = "Auto";
					txtnotransaksi.enabled = false;
					chkauto.selected = true; 
					chkauto.enabled = false;
				}else if(vNotransaksi == '2'){
					if(chkauto.selected == true){
						if(vNotransaksi == '2' && vDefNotransaksi == '0'){
							txtnotransaksi.text = 'Auto';
							txtnotransaksi.enabled = false;
						}else if(vNotransaksi == '2' && vDefNotransaksi == '1'){
							txtnotransaksi.text = 'Auto';
							txtnotransaksi.enabled = false;
						}
					}else{
						if(vNotransaksi == '2' && vDefNotransaksi == '0'){
							txtnotransaksi.text = '';
							txtnotransaksi.enabled = true;
							txtnotransaksi.setFocus();
						}else if(vNotransaksi == '2' && vDefNotransaksi == '1'){
							txtnotransaksi.text = '';
							txtnotransaksi.enabled = true;
							txtnotransaksi.setFocus();
						}
					}
				}
			}
		}
	}
}