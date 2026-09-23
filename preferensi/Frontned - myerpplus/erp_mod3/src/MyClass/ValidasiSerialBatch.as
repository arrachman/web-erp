package MyClass
{
	import mx.collections.ArrayCollection;
	import mx.controls.Alert;
	import mx.core.FlexGlobals;
	
	public class ValidasiSerialBatch
	{
		public var gudangawal : String = ""
		public function ValidasiSerialBatch()
		{
		}
		public function f_ValidasiSerialBatch(gudangbarang:String, gudangbatchserial:String, arrayserialbatch :ArrayCollection):ArrayCollection{
			if(arrayserialbatch.length > 0){
				if(gudangbarang != gudangbatchserial){
					Alert.show("Data Batch/Serial yg sudah dipilih tidak sama dengan gudang asal. Silahkan isi Batch/Serial ulang.")
//					Alert.show(FlexGlobals.topLevelApplication.l('Data Batch/Serial yg sudah dipilih tidak sama dengan gudang asal. Jika lanjut maka data batch/serial akan direset, lanjutkan?'), FlexGlobals.topLevelApplication.l('Konfirmasi'), 3, null, function(e:Object):void{	
//						if(e.detail == Alert.YES){
//							if(arraybatchserial.length > 0){
//								arraybatchserial.removeAll();
//							}
//						}else{
//							gudangawal = gudangbatchserial;
//						}
//					});
						arrayserialbatch.removeAll();
				}
			}
			return	arrayserialbatch;
		}
	}
}