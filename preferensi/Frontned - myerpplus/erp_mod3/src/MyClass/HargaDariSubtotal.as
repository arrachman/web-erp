package MyClass
{
	import mx.controls.Alert;
	import mx.core.FlexGlobals;
	
	import spark.components.CheckBox;
	import spark.components.DataGrid;
	import spark.components.TextInput;
	
	public class HargaDariSubtotal
	{
		
		public function HargaDariSubtotal()
		{
		}
		
		public function hitungHargaDariSubtotal(dg:DataGrid, adadiskon:Boolean=true):void
		{
			var dataGrid:Object={}, diskonbertingkat:String='';
			dataGrid = dg.dataProvider.getItemAt(dg.selectedCell.rowIndex);
				if(adadiskon == true){
					diskonbertingkat = dataGrid.diskon;
					var arr:Array;
					arr = diskonbertingkat.split("+")
					if(arr.length > 1){
						Alert.show('Subtotal tidak bisa diganti','Informasi');
						dataGrid.subtotal = (Number(dataGrid.jml)*Number(dataGrid.harga)) - Number(dataGrid.jmldiskon);
					}else{
						dataGrid.harga = ((Number(dataGrid.subtotal)*100) / (100-dataGrid.diskon)) / Number(dataGrid.jml);
					}
				}else{
					dataGrid.harga = (Number(dataGrid.subtotal) / Number(dataGrid.jml));
				}
		}
	}
}