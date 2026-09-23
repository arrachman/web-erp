package MyClass
{
	public class AdditionalClass
	{
		public function AdditionalClass()
		{
		}
		
		public function f_JmlPajak(hargatermasukpajak:Boolean, nilaipajakx:Number, subtotal:Number, nilaipajak1:Number = 0, nilaipajak2:Number = 0):Number{
			var jmlpajak:Number = 0;
			if(hargatermasukpajak == true){
				// jmlpajak = (nilaipajakx/100) * (subtotal /( 1 + (nilaipajak1 / 100) + (nilaipajak2 / 100))) // perhitungan pajak lama
				jmlpajak = (nilaipajakx/100) * (subtotal /( 1 + (nilaipajak1 / 100)))
			}else{
				jmlpajak = (nilaipajakx/100) * (subtotal)
			}
			
			return jmlpajak;
		}
	}
}