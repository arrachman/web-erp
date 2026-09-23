package MyClass
{
	import flash.external.ExternalInterface;
	
	import mx.controls.Alert;

	public class StiReport
	{
		public var WebAccessKey:String, Filter:String, OrderBy:String, GroupBy:String, RQuery:String, Param:String, Title:String, UserNama:String, Sumber:String, NamaPerusahaan:String;
		public var Module:int, Menu:int, Item:int, Extension:int, UserID:int, IDTransaksi:int;
		
		public function StiReport(param:String)
		{
			ExternalInterface.call('print', param);
		}
	}
}