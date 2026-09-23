import com.adobe.utils.NumberFormatter;

import flash.events.FocusEvent;
import flash.events.KeyboardEvent;
import flash.events.MouseEvent;
import flash.ui.Keyboard;

import mx.controls.Alert;
import mx.core.FlexGlobals;

import spark.components.Image;
import spark.events.TextOperationEvent;
import spark.formatters.NumberFormatter;

public var i:int;
public function F_SetNavigasiKeyboard(ar:Array):void
{
	var i:int;
	var x:int=ar.length;
	for(i=0;i<x;i++)
	{
		ar[i].addEventListener(KeyboardEvent.KEY_DOWN,Obj_keyDownHandler);	
	}
	
	function Obj_keyDownHandler(event:KeyboardEvent):void
	{
		if((event.keyCode==Keyboard.DOWN)||(event.keyCode==Keyboard.ENTER))
		{
			focusManager.getNextFocusManagerComponent().setFocus();
		}
		else if(event.keyCode==Keyboard.UP)
		{
			focusManager.getNextFocusManagerComponent(true).setFocus();
		}
	}	
}


public function f_refreshIDmodule(obj:Object = null):void
{
	if(obj == null){
		if(FlexGlobals.topLevelApplication.idmod.split('DynamicLoader').length > 1)
			FlexGlobals.topLevelApplication.idmod = FlexGlobals.topLevelApplication.f_cariIdmod(FlexGlobals.topLevelApplication.MySuperTab.getChildAt(FlexGlobals.topLevelApplication.MySuperTab.selectedIndex)['label']);
	}else{
		FlexGlobals.topLevelApplication.m.a = String(obj).split('.');
		if(FlexGlobals.topLevelApplication.m.a[0] == 'MyERPPlus')
			
			FlexGlobals.topLevelApplication.idmod = FlexGlobals.topLevelApplication.m.a[7];
		else if(FlexGlobals.topLevelApplication.m.a[0].split('winPopUp')['length'] > 1)
			FlexGlobals.topLevelApplication.idmod = FlexGlobals.topLevelApplication.m.a[5];
		else /*if(module != null){
			if(module.split('DynamicLoader').length == 0 && module.split('popUpLoader').length == 0)
			module = idmod;
			}else*/ FlexGlobals.topLevelApplication.idmod = FlexGlobals.topLevelApplication.idmod;
	}
}

public function F_txtpencarian(arr:Array):void{
	f_refreshIDmodule(arr[0]);
	FlexGlobals.topLevelApplication.ob = FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child;
	for(i=0;i<arr.length;i++)
		add(arr[i])
	
	function add(txt:Object):void{
		txt.addEventListener(FocusEvent.FOCUS_IN, f_focusin);
		txt.addEventListener(FocusEvent.FOCUS_OUT, f_focusout);  
		txt.addEventListener(KeyboardEvent.KEY_DOWN, f_keydown);
		
		FlexGlobals.topLevelApplication.m.img = new Image();
		FlexGlobals.topLevelApplication.m.img.left = (txt.left+txt.width+3);
		FlexGlobals.topLevelApplication.m.img.top = (txt.top+3); 
		FlexGlobals.topLevelApplication.m.img.source = FlexGlobals.topLevelApplication.tbfind; 
		f_cariklik(FlexGlobals.topLevelApplication.m.img, txt);
		txt.parent.addElement(FlexGlobals.topLevelApplication.m.img); 
		
		function f_focusin(e:FocusEvent):void{
			FlexGlobals.topLevelApplication.vpIdTxt = txt;
			if(FlexGlobals.topLevelApplication.vpSetIn == true){
				FlexGlobals.topLevelApplication.vpVarCurrent = txt.text;			
				FlexGlobals.topLevelApplication.vpSetIn = false;
			}
		}
		function f_focusout(e:FocusEvent):void{
			if(FlexGlobals.topLevelApplication.vpOnEnter == false)FlexGlobals.topLevelApplication.vpFocusEvent = false;
			if(txt.text == '' && txt.id != null){
				if(FlexGlobals.topLevelApplication.ob.hasOwnProperty('f_kosongkanText'))FlexGlobals.topLevelApplication.ob.f_kosongkanText(txt.id);
				FlexGlobals.topLevelApplication.vpSetIn = true;
			}else if(txt.text != FlexGlobals.topLevelApplication.vpVarCurrent){
				if(FlexGlobals.topLevelApplication.vpFocusEnter == false){
					FlexGlobals.topLevelApplication.F_cari(txt.id);	
					FlexGlobals.topLevelApplication.vpSetIn = true;
				}
			}else if(txt.text == FlexGlobals.topLevelApplication.vpVarCurrent){
				FlexGlobals.topLevelApplication.vpSetIn = true;
			}
		}
		function f_keydown(e:KeyboardEvent):void{
			switch(e.keyCode){
				case Keyboard.F12:FlexGlobals.topLevelApplication.vpFocusEnter = true;FlexGlobals.topLevelApplication.vpFocusEvent=true;FlexGlobals.topLevelApplication.F_popUpCari(txt.id);break;
				case Keyboard.UP:FlexGlobals.topLevelApplication.focusMgr(true);break;
				case Keyboard.DOWN:focusMgr();break;
				case Keyboard.ENTER:FlexGlobals.topLevelApplication.vpFocusEnter = true;FlexGlobals.topLevelApplication.vpFocusEvent=true;FlexGlobals.topLevelApplication.vpOnEnter=true;
					if(FlexGlobals.topLevelApplication.vpVarCurrent != txt.text && txt.text != ''){
						FlexGlobals.topLevelApplication.F_cari(txt.id);
					}else if(FlexGlobals.topLevelApplication.vpVarCurrent == txt.text)
						focusMgr();
					break;				
			}
		}
		function f_cariklik(img:Image, txt:Object):void{
			img.addEventListener(MouseEvent.CLICK, f_kliklistener);
			function f_kliklistener(e:MouseEvent):void{
				txt.setFocus();
				FlexGlobals.topLevelApplication.F_popUpCari(txt.id);
			} 
		}
	}
}

public function focusMgr(backward:Boolean = false):void
{
	focusManager.getNextFocusManagerComponent(backward).setFocus();
}

public function F_DateFieldStandard(arNamaDateField:Array, modetxet:Boolean = false):void
{
	for(i=0;i<arNamaDateField.length;i++)
	{
		arNamaDateField[i].restrict = ". -/0-9";
		if(modetxet == false){
			arNamaDateField[i].formatString = FlexGlobals.topLevelApplication.m.formatDate.toUpperCase();
			arNamaDateField[i].editable = true;
		}
		arNamaDateField[i].addEventListener(FocusEvent.FOCUS_OUT,Df_focusOutHandler);
	}
	
	F_SetNavigasiKeyboard(arNamaDateField);
	
	function Df_focusOutHandler(event:FocusEvent):void
	{
		//variable pendukung
		var ob:Object = event.currentTarget;
		var dt:String = ob.text;
		var d:Array;
		
		//distandarkan format tanggalnya
		dt = dt.split(" ").join("/");
		dt = dt.split("-").join("/");
		d = dt.split('/');
		
		if(d.length == 1)
			switch(dt.length){
				case 1:f_valid("0"+dt, 1, 1900);break;
				case 2:f_valid(dt, 1, 1900);break;
				case 4:f_valid(dt.slice(0,2), dt.slice(2,4), 1900);break;
				case 6:f_valid(dt.slice(0,2), dt.slice(2,4), dt.slice(4,6));break;
				case 8:f_valid(dt.slice(0,2), dt.slice(2,4), dt.slice(4,8));break;
				default:ob.text = '';break;
			}
		else if(d.length == 2)
			f_valid(d[0], d[1], 1900);
		else if(d.length == 3)
			f_valid(d[0], d[1], d[2]);
		else ob.text = '';
		
		function f_valid(dd:int, mm:int, yyyy:int):void
		{
			var d:int, d1:String, d2:String;
			if(mm > 12)mm = 12;
			else if(mm < 1)mm = 1;
			if(yyyy == 0)yyyy = 1900;
			else if(String(yyyy).length < 4)yyyy = 1900;
			else if(yyyy > 2100) yyyy = 2100;
			//			else if(yyyy < 1900) yyyy = 1900;
			d = FlexGlobals.topLevelApplication.F_getDayCount(yyyy, mm);
			if(dd > d) dd = d;
			else if(dd < 1) dd = 1;
			d1 = String(dd);
			d2 = String(mm);
			if(d1.length == 1)d1 = "0"+d1;
			if(d2.length == 1)d2 = "0"+d2;
			ob.text = d1+"/"+d2+"/"+yyyy;
		}		
	}
}
public function F_TextInputNumberDesimalMode(arTextInput:Array):void
{
	f_refreshIDmodule(arTextInput[0]);
	for (i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].restrict = FlexGlobals.topLevelApplication.m.nfNominal.decimalSeparator+"0-9,.";
		arTextInput[i].addEventListener(FocusEvent.FOCUS_IN, TextfocusInHandler);
		arTextInput[i].addEventListener(spark.events.TextOperationEvent.CHANGE, TextChangeHandler);
		arTextInput[i].addEventListener(FocusEvent.FOCUS_OUT, TextfocusOutHandler);
		arTextInput[i].setStyle("textAlign","right");
		arTextInput[i].text = FlexGlobals.topLevelApplication.m.nfNominal.format("0");
		arTextInput[i].maxChars = 15;
		FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[arTextInput[i].id] = 0;
	}
	F_SetNavigasiKeyboard(arTextInput);
	function TextfocusInHandler(event:FocusEvent):void
	{
		event.currentTarget.text = FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[event.currentTarget.id];
	}
	
	function TextChangeHandler(event:TextOperationEvent):void
	{
		FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[event.currentTarget.id] = event.currentTarget.text;
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[event.currentTarget.id] = String(FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[event.currentTarget.id]).split(FlexGlobals.topLevelApplication.m.nfNominal.decimalSeparator).join('.');
		event.currentTarget.text = FlexGlobals.topLevelApplication.m.nfNominal.format(event.currentTarget.text);
	}
}
public function F_TextInputNumberOnlyMode(arTextInput:Array):void
{
	f_refreshIDmodule(arTextInput[0]);
	for (i=0;i<arTextInput.length;i++)
	{
		arTextInput[i].restrict = "0-9";
		arTextInput[i].addEventListener(FocusEvent.FOCUS_OUT,TextfocusOutHandler);
		arTextInput[i].addEventListener(spark.events.TextOperationEvent.CHANGE, TextChangeHandler);
		arTextInput[i].addEventListener(FocusEvent.FOCUS_IN,TextfocusInHandler);
		arTextInput[i].setStyle("textAlign","right");
		arTextInput[i].text = FlexGlobals.topLevelApplication.m.nfNumber.format('0');
		arTextInput[i].maxChars = 15;
		FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[arTextInput[i].id] = 0;
	}
	
	F_SetNavigasiKeyboard(arTextInput);
	
	function TextfocusInHandler(event:FocusEvent):void
	{
		event.currentTarget.text = FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[event.currentTarget.id];
	}
	
	function TextChangeHandler(event:TextOperationEvent):void
	{
		FlexGlobals.topLevelApplication.m[FlexGlobals.topLevelApplication.idmod].child.o[event.currentTarget.id] = event.currentTarget.text;
	}
	
	function TextfocusOutHandler(event:FocusEvent):void
	{
		event.currentTarget.text = FlexGlobals.topLevelApplication.m.nfNumber.format(event.currentTarget.text);
	}
}
public function F_formatNilai(data:String):String
{
	var m :Object={};
	m.nfNilai = FlexGlobals.topLevelApplication.m.nfNumber;
	if(data == null || data == '')
		data = '0';
	
	data = data.split(m.nfNilai.decimalSeparator).join('.');
	FlexGlobals.topLevelApplication.m.temp = data.split('+');
	if(FlexGlobals.topLevelApplication.m.temp.length > 1){
		// Jika dison Bertingkat 
		FlexGlobals.topLevelApplication.m.s = m.nfNilai.format(FlexGlobals.topLevelApplication.m.temp[0]);
		for(i=1;i<FlexGlobals.topLevelApplication.m.temp.length;i++)
			FlexGlobals.topLevelApplication.m.s += "+"+m.nfNilai.format(FlexGlobals.topLevelApplication.m.temp[i]);
		return FlexGlobals.topLevelApplication.m.s;
	}else
		return m.nfNilai.format(data);
}