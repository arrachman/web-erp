import flash.globalization.NumberFormatter;

import mx.collections.ArrayCollection;

import org.flexunit.runners.ParentRunner;

import spark.formatters.NumberFormatter;

[Bindable]public var arrtahun:ArrayCollection= new ArrayCollection;
[Bindable]public var arrbulan:ArrayCollection = new ArrayCollection;
[Bindable]public var arrdg:ArrayCollection = new ArrayCollection;

public var ob:Object, thn:int, bln:int;
public var bulan:Array = new Array('Januari', 'Februari', 'Maret', 'April', 'Mei', 'Juni', 'Juli', 'Agustus', 'September', 'Oktober', 'November', 'Desember');
public var fmt:spark.formatters.NumberFormatter;
public function nf(n:Number):String{
	return fmt.format(n);
}
public function ldm():int{
	return getDayCount(int(cmbtahun.textInput.text), (cmbbulan.selectedIndex+1));
}
public function getDayCount(year:int, month:int):int{
	var d:Date=new Date(year, month, 0);
	return d.getDate();
}

public function script():void{
	fmt = new spark.formatters.NumberFormatter();
	fmt.decimalSeparator = ".";
	fmt.fractionalDigits = 2;
	fmt.groupingSeparator = ",";
	thn = parentApplication.DefaultTanggalforDB.split('-')[0];
	bln = parentApplication.DefaultTanggalforDB.split('-')[1];
	isitahun(arrtahun);
	isibulan(arrbulan);
	cmbtahun.dataProvider = new ArrayCollection(arrtahun.toArray());
	cmbbulan.dataProvider = new ArrayCollection(arrbulan.toArray());
	parentApplication.F_ComboBoxStandard([cmbtahun, cmbbulan]);
	ffilter();
}

public function isitahun(arr:ArrayCollection):void{
	arr.removeAll();
	var nowyear:int = parentApplication.DefaultTanggalforDB.split('-')[0];
	for(var i:int=(nowyear-10);i<=(nowyear+10);i++)
		arrtahun.addItem({l:i});
}

public function isibulan(arr:ArrayCollection):void{
	arr.removeAll();
	for(var i:int=0;i<bulan.length;i++)
		arr.addItem({l:bulan[i]});
}


protected function f_formatRupiah(item:Object, column:GridColumn):String
{
	return fmt.format(item.omzet);
}

public function f_FormatUang(val:String):String
{
	return fmt.format(Number(val));
}

public function getItemIndexByProperty(array:ArrayCollection, property:String, value:String):Number
{
	for (var i:Number = 0; i < array.length; i++)
	{
		var obj:Object = Object(array[i])
		if (obj[property] == value)
			return i;
	}
	return -1;
	
	array.getItemIndex();
}

public function ws(subject:String = ''):void{
	cektahunbulan();
	parentApplication.subject = "c-"+subject;
}

public function cektahunbulan():void{
	if(cmbtahun.textInput.text != '')thn = int(cmbtahun.textInput.text);
	bln = (cmbbulan.selectedIndex+1);
}

public function ffilter():void{
	if(parentApplication.FormFilter != ''){
		var a:Array = parentApplication.FormFilter.split('|');
		cmbtahun.selectedIndex = getItemIndexByProperty(arrtahun, 'l', a[0]);
		cmbbulan.selectedIndex = (int(a[1])-1);
	}else{
		cmbtahun.selectedIndex = getItemIndexByProperty(arrtahun, 'l', parentApplication.DefaultTanggalforDB.split('-')[0]);
		cmbbulan.selectedIndex = int(parentApplication.DefaultTanggalforDB.split('-')[1])-1;
	}
}

private function gnf(item:Object , column:GridColumn):String
{
	return fmt.format(item [column.dataField]);
}