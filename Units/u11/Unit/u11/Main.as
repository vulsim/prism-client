import flash.external.ExternalInterface;

ExternalInterface.addCallback("SetObjectParam", null, SetObjectParam);
ExternalInterface.addCallback("Initialize", null, Initialize);

/////////////////////////////////////////////////////////////////////
// Инициализация

ScWidth = 1024; //this._width;
ScHeight = 768; //this._height;

Screen1._visible = 0;
Screen2._visible = 0;

info_leadin1._visible = 0;
info_leadin2._visible = 0;

info_rect1._visible = 0;
info_rect2._visible = 0;
info_rect3._visible = 0;

info_lsw1._visible = 0;
info_lsw2._visible = 0;
info_lsw3._visible = 0;
info_lsw4._visible = 0;
info_lsw5._visible = 0;
info_lsw9._visible = 0;

/////////////////////////////////////////////////////////////////////

var leadin1_state = 0;
var leadin1_state_in_switch = 0;

var leadin2_state = 0;
var leadin2_state_in_switch = 0;

var lsw1_state = 0;
var lsw1_state_qs_switch = 0;
var lsw1_state_spare_switch = 0;

var lsw2_state = 0;
var lsw2_state_qs_switch = 0;
var lsw2_state_spare_switch = 0;

var lsw3_state = 0;
var lsw3_state_qs_switch = 0;
var lsw3_state_spare_switch = 0;

var lsw4_state = 0;
var lsw4_state_qs_switch = 0;
var lsw4_state_spare_switch = 0;

var lsw5_state = 0;
var lsw5_state_qs_switch = 0;
var lsw5_state_spare_switch = 0;

var lsw9_state = 0;
var lsw9_state_qs_switch = 0;

/////////////////////////////////////////////////////////////////////

LeadIn1.onRelease = function() 
{
	fscommand("CtrlLeadin1", 1);	
}

LeadIn2.onRelease = function() 
{
	fscommand("CtrlLeadin2", 1);	
}

Rect1.onRelease = function() 
{
	fscommand("CtrlRect1", 1);	
}

Rect2.onRelease = function() 
{
	fscommand("CtrlRect2", 1);
}

Rect3.onRelease = function() 
{
	fscommand("CtrlRect3", 1);
}

Lsw1.onRelease = function() 
{
	fscommand("CtrlLsw1", 1);
}

Lsw2.onRelease = function() 
{
	fscommand("CtrlLsw2", 1);
}

Lsw3.onRelease = function() 
{
	fscommand("CtrlLsw3", 1);
}

Lsw4.onRelease = function() 
{
	fscommand("CtrlLsw4", 1);
}

Lsw5.onRelease = function() 
{
	fscommand("CtrlLsw5", 1);
}

Lsw9.onRelease = function() 
{
	fscommand("CtrlLsw9", 1);
}

LeadIn1.onRollOver = function()
{	
	info_leadin1.ShowPanel(LeadIn1._x+LeadIn1._width/2, LeadIn1._y+LeadIn1._height/2, LeadIn1._width/2, LeadIn1._height/2, ScWidth, ScHeight);
}

LeadIn2.onRollOver = function()
{
	info_leadin2.ShowPanel(LeadIn2._x+LeadIn1._width/2, LeadIn2._y+LeadIn1._height/2, LeadIn2._width/2, LeadIn2._height/2, ScWidth, ScHeight);
}

Rect1.onRollOver = function()
{	
	info_rect1.ShowPanel(Rect1._x+Rect1._width/2, Rect1._y+Rect1._height/2, Rect1._width/2, Rect1._height/2, ScWidth, ScHeight);
}

Rect2.onRollOver = function()
{	
	info_rect2.ShowPanel(Rect2._x+Rect2._width/2, Rect2._y+Rect2._height/2, Rect2._width/2, Rect2._height/2, ScWidth, ScHeight);
}

Rect3.onRollOver = function()
{	
	info_rect3.ShowPanel(Rect3._x+Rect3._width/2, Rect3._y+Rect3._height/2, Rect3._width/2, Rect3._height/2, ScWidth, ScHeight);
}

Lsw1.onRollOver = function()
{	
	info_lsw1.ShowPanel(Lsw1._x+Lsw1._width/2, Lsw1._y+Lsw1._height/2, Lsw1._width/2, Lsw1._height/2, ScWidth, ScHeight);
}

Lsw2.onRollOver = function()
{		
	info_lsw2.ShowPanel(Lsw2._x+Lsw2._width/2, Lsw2._y+Lsw2._height/2, Lsw2._width/2, Lsw2._height/2, ScWidth, ScHeight);
}

Lsw3.onRollOver = function()
{		
	info_lsw3.ShowPanel(Lsw3._x+Lsw3._width/2, Lsw3._y+Lsw3._height/2, Lsw3._width/2, Lsw3._height/2, ScWidth, ScHeight);
}

Lsw4.onRollOver = function()
{	
	info_lsw4.ShowPanel(Lsw4._x+Lsw4._width/2, Lsw4._y+Lsw4._height/2, Lsw4._width/2, Lsw4._height/2, ScWidth, ScHeight);
}

Lsw5.onRollOver = function()
{
	info_lsw5.ShowPanel(Lsw5._x+Lsw5._width/2, Lsw5._y+Lsw5._height/2, Lsw5._width/2, Lsw5._height/2, ScWidth, ScHeight);
}

Lsw9.onRollOver = function()
{	
	info_lsw9.ShowPanel(Lsw9._x+Lsw9._width/2, Lsw9._y+Lsw9._height/2, Lsw9._width/2, Lsw9._height/2, ScWidth, ScHeight);
}

LeadIn1.onRollOut = function()
{
	info_leadin1._visible = 0;
}

LeadIn2.onRollOut = function()
{
	info_leadin2._visible = 0;
}

Rect1.onRollOut = function()
{
	info_rect1._visible = 0;
}

Rect2.onRollOut = function()
{
	info_rect2._visible = 0;
}

Rect3.onRollOut = function()
{
	info_rect3._visible = 0;
}

Lsw1.onRollOut = function()
{
	info_lsw1._visible = 0;
}

Lsw2.onRollOut = function()
{
	info_lsw2._visible = 0;
}

Lsw3.onRollOut = function()
{
	info_lsw3._visible = 0;
}

Lsw4.onRollOut = function()
{
	info_lsw4._visible = 0;
}

Lsw5.onRollOut = function()
{
	info_lsw5._visible = 0;
}

Lsw9.onRollOut = function()
{
	info_lsw9._visible = 0;
}

function Initialize() {
	
	general_param1.SetRegion(200, 25);
	general_param2.SetRegion(200, 25);
	general_param3.SetRegion(200, 25);
	general_param4.SetRegion(200, 25);
	general_param5.SetRegion(200, 25);
	general_param6.SetRegion(200, 25);
	general_param7.SetRegion(200, 25);
	
	info_leadin1.SetCaption("Рабочий ввод");
	info_leadin1.FieldState(1, 1);
	info_leadin1.FieldState(2, 1);
	info_leadin1.FieldState(3, 1);
	info_leadin1.FieldState(4, 1);
	
	info_leadin2.SetCaption("Резервн. ввод");
	info_leadin2.FieldState(1, 1);
	info_leadin2.FieldState(2, 1);
	info_leadin2.FieldState(3, 1);
	info_leadin2.FieldState(4, 1);
	
	info_rect1.SetCaption("Выпр. агрегат №1");
	info_rect1.FieldState(1, 1);
	info_rect1.FieldState(2, 1);
	info_rect1.FieldState(3, 1);
	info_rect1.FieldState(4, 1);
	info_rect1.FieldState(5, 1);
	info_rect1.FieldState(6, 1);
	info_rect1.FieldState(7, 1);
	info_rect1.FieldState(8, 1);
	info_rect1.FieldState(9, 1);
	info_rect1.FieldState(10, 1);
	
	info_rect2.SetCaption("Выпр. агрегат №2");
	info_rect2.FieldState(1, 1);
	info_rect2.FieldState(2, 1);
	info_rect2.FieldState(3, 1);
	info_rect2.FieldState(4, 1);
	info_rect2.FieldState(5, 1);
	info_rect2.FieldState(6, 1);
	info_rect2.FieldState(7, 1);
	info_rect2.FieldState(8, 1);
	info_rect2.FieldState(9, 1);
	info_rect2.FieldState(10, 1);
	
	info_rect3.SetCaption("Выпр. агрегат №3");
	info_rect3.FieldState(1, 1);
	info_rect3.FieldState(2, 1);
	info_rect3.FieldState(3, 1);
	info_rect3.FieldState(4, 1);
	info_rect3.FieldState(5, 1);
	info_rect3.FieldState(6, 1);
	info_rect3.FieldState(7, 1);
	info_rect3.FieldState(8, 1);
	info_rect3.FieldState(9, 1);
	info_rect3.FieldState(10, 1);
	
	info_lsw1.SetCaption("Лин. выключатель №1");
	info_lsw1.FieldState(1, 1);
	info_lsw1.FieldState(2, 1);
	info_lsw1.FieldState(3, 1);
	info_lsw1.FieldState(4, 1);
	info_lsw1.FieldState(5, 1);
	info_lsw1.FieldState(6, 1);
	info_lsw1.FieldState(7, 1);

	info_lsw2.SetCaption("Лин. выключатель №2");
	info_lsw2.FieldState(1, 1);
	info_lsw2.FieldState(2, 1);
	info_lsw2.FieldState(3, 1);
	info_lsw2.FieldState(4, 1);
	info_lsw2.FieldState(5, 1);
	info_lsw2.FieldState(6, 1);
	info_lsw2.FieldState(7, 1);

	info_lsw3.SetCaption("Лин. выключатель №3");
	info_lsw3.FieldState(1, 1);
	info_lsw3.FieldState(2, 1);
	info_lsw3.FieldState(3, 1);
	info_lsw3.FieldState(4, 1);
	info_lsw3.FieldState(5, 1);
	info_lsw3.FieldState(6, 1);
	info_lsw3.FieldState(7, 1);

	info_lsw4.SetCaption("Лин. выключатель №4");
	info_lsw4.FieldState(1, 1);
	info_lsw4.FieldState(2, 1);
	info_lsw4.FieldState(3, 1);
	info_lsw4.FieldState(4, 1);
	info_lsw4.FieldState(5, 1);
	info_lsw4.FieldState(6, 1);
	info_lsw4.FieldState(7, 1);

	info_lsw5.SetCaption("Лин. выключатель №5");
	info_lsw5.FieldState(1, 1);
	info_lsw5.FieldState(2, 1);
	info_lsw5.FieldState(3, 1);
	info_lsw5.FieldState(4, 1);
	info_lsw5.FieldState(5, 1);
	info_lsw5.FieldState(6, 1);
	info_lsw5.FieldState(7, 1);

	info_lsw9.SetCaption("Запасной выключатель");
	info_lsw9.FieldState(1, 1);
	info_lsw9.FieldState(2, 1);
	info_lsw9.FieldState(3, 1);
	info_lsw9.FieldState(4, 1);
		
	AmpLeadIn1.SetLimits(800, 900, 1000);
	AmpLeadIn2.SetLimits(800, 900, 1000);
	AmpRect1.SetLimits(800, 900, 1000);
	AmpRect2.SetLimits(800, 900, 1000);
	AmpRect3.SetLimits(800, 900, 1000);
	AmpLsw1.SetLimits(800, 900, 1000);
	AmpLsw2.SetLimits(800, 900, 1000);
	AmpLsw3.SetLimits(800, 900, 1000);
	AmpLsw4.SetLimits(800, 900, 1000);
	AmpLsw5.SetLimits(800, 900, 1000);
	AmpLsw9.SetLimits(1100, 1250, 1400);	
	
	AmpLeadIn1.Rms(0);
	AmpLeadIn2.Rms(0);
	AmpRect1.Rms(0);
	AmpRect2.Rms(0);
	AmpRect3.Rms(0);
	AmpLsw1.Rms(0);
	AmpLsw2.Rms(0);
	AmpLsw3.Rms(0);
	AmpLsw4.Rms(0);
	AmpLsw5.Rms(0);
	AmpLsw9.Rms(0);
	
	AmpLeadIn1.Peak(0);
	AmpLeadIn2.Peak(0);
	AmpRect1.Peak(0);
	AmpRect2.Peak(0);
	AmpRect3.Peak(0);
	AmpLsw1.Peak(0);
	AmpLsw2.Peak(0);
	AmpLsw3.Peak(0);
	AmpLsw4.Peak(0);
	AmpLsw5.Peak(0);
	AmpLsw9.Peak(0);
	
	Bus2.gotoAndStop(1);
}

function UpdateLeadin1State() {
	switch (leadin1_state_in_switch) {
		case 2:
			if (leadin1_state > 1) {
				LeadIn1.gotoAndStop(leadin1_state + 1);
			} else {
				LeadIn1.gotoAndStop(2);
			}			
			break;
		case 3:
			if (leadin1_state > 1) {
				LeadIn1.gotoAndStop(leadin1_state + 5);
			} else {
				LeadIn1.gotoAndStop(6);
			}
			break;
		default:
			LeadIn1.gotoAndStop(1);
			break;
	}
}

function UpdateLeadin2State() {
	switch (leadin2_state_in_switch) {
		case 2:
			if (leadin2_state > 1) {
				LeadIn2.gotoAndStop(leadin2_state + 1);
			} else {
				LeadIn2.gotoAndStop(2);
			}			
			break;
		case 3:
			if (leadin2_state > 1) {
				LeadIn2.gotoAndStop(leadin2_state + 5);
			} else {
				LeadIn2.gotoAndStop(6);
			}
			break;
		default:
			LeadIn2.gotoAndStop(1);
			break;
	}
}

function UpdateLsw1State() {
	switch (lsw1_state_spare_switch) {
		case 2:
			if (lsw1_state > 1) {
				Lsw1.gotoAndStop(lsw1_state + 9);
			} else {
				Lsw1.gotoAndStop(10);
			}			
			break;
		default:
			switch (lsw1_state_qs_switch) {
				case 2:
					if (lsw1_state > 1) {
						Lsw1.gotoAndStop(lsw1_state + 5);
					} else {
						Lsw1.gotoAndStop(6);
					}
					break;
				default:
					if (lsw1_state > 1) {
						Lsw1.gotoAndStop(lsw1_state + 1);
					} else {
						Lsw1.gotoAndStop(1);
					}
					break
			}			
			break;
	}
}

function UpdateLsw2State() {
	switch (lsw2_state_spare_switch) {
		case 2:
			if (lsw2_state > 1) {
				Lsw2.gotoAndStop(lsw2_state + 9);
			} else {
				Lsw2.gotoAndStop(10);
			}			
			break;
		default:
			switch (lsw2_state_qs_switch) {
				case 2:
					if (lsw2_state > 1) {
						Lsw2.gotoAndStop(lsw2_state + 5);
					} else {
						Lsw2.gotoAndStop(6);
					}
					break;
				default:
					if (lsw2_state > 1) {
						Lsw2.gotoAndStop(lsw2_state + 1);
					} else {
						Lsw2.gotoAndStop(1);
					}
					break
			}			
			break;
	}
}

function UpdateLsw3State() {
	switch (lsw3_state_spare_switch) {
		case 2:
			if (lsw3_state > 1) {
				Lsw3.gotoAndStop(lsw3_state + 9);
			} else {
				Lsw3.gotoAndStop(10);
			}			
			break;
		default:
			switch (lsw3_state_qs_switch) {
				case 2:
					if (lsw3_state > 1) {
						Lsw3.gotoAndStop(lsw3_state + 5);
					} else {
						Lsw3.gotoAndStop(6);
					}
					break;
				default:
					if (lsw3_state > 1) {
						Lsw3.gotoAndStop(lsw3_state + 1);
					} else {
						Lsw3.gotoAndStop(1);
					}
					break
			}			
			break;
	}
}

function UpdateLsw4State() {
	switch (lsw4_state_spare_switch) {
		case 2:
			if (lsw4_state > 1) {
				Lsw4.gotoAndStop(lsw4_state + 9);
			} else {
				Lsw4.gotoAndStop(10);
			}			
			break;
		default:
			switch (lsw4_state_qs_switch) {
				case 2:
					if (lsw4_state > 1) {
						Lsw4.gotoAndStop(lsw4_state + 5);
					} else {
						Lsw4.gotoAndStop(6);
					}
					break;
				default:
					if (lsw4_state > 1) {
						Lsw4.gotoAndStop(lsw4_state + 1);
					} else {
						Lsw4.gotoAndStop(1);
					}
					break
			}			
			break;
	}
}

function UpdateLsw5State() {
	switch (lsw5_state_spare_switch) {
		case 2:
			if (lsw5_state > 1) {
				Lsw5.gotoAndStop(lsw5_state + 9);
			} else {
				Lsw5.gotoAndStop(10);
			}			
			break;
		default:
			switch (lsw5_state_qs_switch) {
				case 2:
					if (lsw5_state > 1) {
						Lsw5.gotoAndStop(lsw5_state + 5);
					} else {
						Lsw5.gotoAndStop(6);
					}
					break;
				default:
					if (lsw5_state > 1) {
						Lsw5.gotoAndStop(lsw5_state + 1);
					} else {
						Lsw5.gotoAndStop(1);
					}
					break
			}			
			break;
	}
}

function UpdateLsw9State() {
	switch (lsw9_state_qs_switch) {
		case 2:
			if (lsw9_state > 1) {
				Lsw9.gotoAndStop(lsw9_state + 5);
			} else {
				Lsw9.gotoAndStop(6);
			}
			Bus2.gotoAndStop(2);
			break;
		default:
			if (lsw9_state > 1) {
				Lsw9.gotoAndStop(lsw9_state + 1);
			} else {
				Lsw9.gotoAndStop(1);
			}
			Bus2.gotoAndStop(1);
			break
	}
}

function SetObjectParam(object, param, value) {
	
	switch (object) {
		
		case "view":
			switch (param) {
				case "avaliable":
					Screen1._visible = value;
					break;
				case "leadin1_state":
					leadin1_state = value;
					UpdateLeadin1State();
					break;
				case "leadin1_state_in_switch":
					leadin1_state_in_switch = value;
					info_leadin1.FieldState(1, value);
					UpdateLeadin1State();					
					break;
				case "leadin1_state_tc_switch":
					info_leadin1.FieldState(7, value);
					break;
				case "leadin1_alarm_in_switch_fault":
					info_leadin1.FieldState(2, value);
					break;
				case "leadin1_alarm_circuit_fault":
					info_leadin1.FieldState(6, value);
					break;
				case "leadin1_alarm_tn_circuit_fault":
					info_leadin1.FieldState(4, value);
					break;
				case "leadin1_alarm_tn_ru6kv_fault":
					info_leadin1.FieldState(5, value);
					break;
				case "leadin1_alarm_tsn_lost_power":
					info_leadin1.FieldState(3, value);
					break;
				case "leadin2_state":
					leadin2_state = value;
					UpdateLeadin2State();
					break;
				case "leadin2_state_in_switch":
					leadin2_state_in_switch = value;
					info_leadin2.FieldState(1, value);
					UpdateLeadin1State();					
					break;
				case "leadin2_state_tc_switch":
					info_leadin2.FieldState(7, value);
					break;
				case "leadin2_alarm_in_switch_fault":
					info_leadin2.FieldState(2, value);
					break;
				case "leadin2_alarm_circuit_fault":
					info_leadin2.FieldState(6, value);
					break;
				case "leadin2_alarm_tn_circuit_fault":
					info_leadin2.FieldState(4, value);
					break;
				case "leadin2_alarm_tn_ru6kv_fault":
					info_leadin2.FieldState(5, value);
					break;
				case "leadin2_alarm_tsn_lost_power":
					info_leadin1.FieldState(3, value);
					general_param7.gotoAndStop(value);
					break;
				case "rect1_state":		
					Rect1.gotoAndStop(value);			
					break;
				case "rect1_state_pa_switch":
					info_rect1.FieldState(1, value);					
					break;
				case "rect1_state_qs_switch":					
					info_rect1.FieldState(2, value);
					break;
				case "rect1_state_qf_switch":					
					info_rect1.FieldState(3, value);
					break;
				case "rect1_state_tc_switch":					
					info_rect1.FieldState(10, value);
					break;
				case "rect1_alarm_circuit_fault":					
					info_rect1.FieldState(9, value);
					break;
				case "rect1_alarm_pa_switch_fault":					
					info_rect1.FieldState(4, value);
					break;
				case "rect1_alarm_rec_fault":					
					info_rect1.FieldState(6, value);
					break;
				case "rect1_alarm_rec_gas_warn":					
					info_rect1.FieldState(8, value);
					break;
				case "rect1_alarm_rec_overload":					
					info_rect1.FieldState(5, value);
					break;
				case "rect1_alarm_rec_rpz600v_fault":					
					info_rect1.FieldState(7, value);
					break;
				case "rect2_state":		
					rect2.gotoAndStop(value);			
					break;
				case "rect2_state_pa_switch":
					info_rect2.FieldState(1, value);					
					break;
				case "rect2_state_qs_switch":					
					info_rect2.FieldState(2, value);
					break;
				case "rect2_state_qf_switch":					
					info_rect2.FieldState(3, value);
					break;
				case "rect2_state_tc_switch":					
					info_rect2.FieldState(10, value);
					break;
				case "rect2_alarm_circuit_fault":					
					info_rect2.FieldState(9, value);
					break;
				case "rect2_alarm_pa_switch_fault":					
					info_rect2.FieldState(4, value);
					break;
				case "rect2_alarm_rec_fault":					
					info_rect2.FieldState(6, value);
					break;
				case "rect2_alarm_rec_gas_warn":					
					info_rect2.FieldState(8, value);
					break;
				case "rect2_alarm_rec_overload":					
					info_rect2.FieldState(5, value);
					break;
				case "rect2_alarm_rec_rpz600v_fault":					
					info_rect2.FieldState(7, value);
					break;
				case "rect3_state":		
					rect3.gotoAndStop(value);			
					break;
				case "rect3_state_pa_switch":
					info_rect3.FieldState(1, value);					
					break;
				case "rect3_state_qs_switch":					
					info_rect3.FieldState(2, value);
					break;
				case "rect3_state_qf_switch":					
					info_rect3.FieldState(3, value);
					break;
				case "rect3_state_tc_switch":					
					info_rect3.FieldState(10, value);
					break;
				case "rect3_alarm_circuit_fault":					
					info_rect3.FieldState(9, value);
					break;
				case "rect3_alarm_pa_switch_fault":					
					info_rect3.FieldState(4, value);
					break;
				case "rect3_alarm_rec_fault":					
					info_rect3.FieldState(6, value);
					break;
				case "rect3_alarm_rec_gas_warn":					
					info_rect3.FieldState(8, value);
					break;
				case "rect3_alarm_rec_overload":					
					info_rect3.FieldState(5, value);
					break;
				case "rect3_alarm_rec_rpz600v_fault":					
					info_rect3.FieldState(7, value);
					break;
				case "lsw1_state":
					lsw1_state = value;
					UpdateLsw1State();		
					break;
				case "lsw1_state_qs_switch":
					lsw1_state_qs_switch = value
					info_lsw1.FieldState(1, value);					
					UpdateLsw1State();
					break;
				case "lsw1_state_qf_switch":
					info_lsw1.FieldState(2, value);					
					break;
				case "lsw1_state_tc_switch":
					info_lsw1.FieldState(7, value);			
					break;
				case "lsw1_state_spare_switch":
					lsw1_state_spare_switch = value
					info_lsw1.FieldState(3, value);					
					UpdateLsw1State();
					break;
				case "lsw1_alarm_short_fault":
					info_lsw1.FieldState(5, value);			
					break;
				case "lsw1_alarm_circuit_fault":
					info_lsw1.FieldState(6, value);
					break;
				case "lsw1_alarm_600v_lost_power":
					info_lsw1.FieldState(4, value);					
					break;
				case "lsw2_state":
					lsw2_state = value;
					UpdateLsw2State();		
					break;
				case "lsw2_state_qs_switch":
					lsw2_state_qs_switch = value
					info_lsw2.FieldState(1, value);					
					UpdateLsw2State();
					break;
				case "lsw2_state_qf_switch":
					info_lsw2.FieldState(2, value);					
					break;
				case "lsw2_state_tc_switch":
					info_lsw2.FieldState(7, value);			
					break;
				case "lsw2_state_spare_switch":
					lsw2_state_spare_switch = value
					info_lsw2.FieldState(3, value);					
					UpdateLsw2State();
					break;
				case "lsw2_alarm_short_fault":
					info_lsw2.FieldState(5, value);			
					break;
				case "lsw2_alarm_circuit_fault":
					info_lsw2.FieldState(6, value);
					break;
				case "lsw2_alarm_600v_lost_power":
					info_lsw2.FieldState(4, value);					
					break;
				case "lsw3_state":
					lsw3_state = value;
					UpdateLsw3State();		
					break;
				case "lsw3_state_qs_switch":
					lsw3_state_qs_switch = value
					info_lsw3.FieldState(1, value);					
					UpdateLsw3State();
					break;
				case "lsw3_state_qf_switch":
					info_lsw3.FieldState(2, value);					
					break;
				case "lsw3_state_tc_switch":
					info_lsw3.FieldState(7, value);			
					break;
				case "lsw3_state_spare_switch":
					lsw3_state_spare_switch = value
					info_lsw3.FieldState(3, value);					
					UpdateLsw3State();
					break;
				case "lsw3_alarm_short_fault":
					info_lsw3.FieldState(5, value);			
					break;
				case "lsw3_alarm_circuit_fault":
					info_lsw3.FieldState(6, value);
					break;
				case "lsw3_alarm_600v_lost_power":
					info_lsw3.FieldState(4, value);					
					break;
				case "lsw4_state":
					lsw4_state = value;
					UpdateLsw4State();		
					break;
				case "lsw4_state_qs_switch":
					lsw4_state_qs_switch = value
					info_lsw4.FieldState(1, value);					
					UpdateLsw4State();
					break;
				case "lsw4_state_qf_switch":
					info_lsw4.FieldState(2, value);					
					break;
				case "lsw4_state_tc_switch":
					info_lsw4.FieldState(7, value);			
					break;
				case "lsw4_state_spare_switch":
					lsw4_state_spare_switch = value
					info_lsw4.FieldState(3, value);					
					UpdateLsw4State();
					break;
				case "lsw4_alarm_short_fault":
					info_lsw4.FieldState(5, value);			
					break;
				case "lsw4_alarm_circuit_fault":
					info_lsw4.FieldState(6, value);
					break;
				case "lsw4_alarm_600v_lost_power":
					info_lsw4.FieldState(4, value);					
					break;
				case "lsw5_state":
					lsw5_state = value;
					UpdateLsw5State();		
					break;
				case "lsw5_state_qs_switch":
					lsw5_state_qs_switch = value
					info_lsw5.FieldState(1, value);					
					UpdateLsw5State();
					break;
				case "lsw5_state_qf_switch":
					info_lsw5.FieldState(2, value);					
					break;
				case "lsw5_state_tc_switch":
					info_lsw5.FieldState(7, value);			
					break;
				case "lsw5_state_spare_switch":
					lsw5_state_spare_switch = value
					info_lsw5.FieldState(3, value);					
					UpdateLsw5State();
					break;
				case "lsw5_alarm_short_fault":
					info_lsw5.FieldState(5, value);			
					break;
				case "lsw5_alarm_circuit_fault":
					info_lsw5.FieldState(6, value);
					break;
				case "lsw5_alarm_600v_lost_power":
					info_lsw5.FieldState(4, value);					
					break;
				case "lsw9_state":
					lsw9_state = value;
					UpdateLsw9State();		
					break;
				case "lsw9_state_qs_switch":
					lsw9_state_qs_switch = value
					info_lsw9.FieldState(1, value);					
					UpdateLsw9State();
					break;
				case "lsw9_state_qf_switch":
					info_lsw9.FieldState(2, value);					
					break;
				case "lsw9_state_tc_switch":
					info_lsw9.FieldState(4, value);			
					break;
				case "lsw9_alarm_circuit_fault":
					info_lsw9.FieldState(3, value);
					break;
				case "general_state_sn_automation":
					general_param1.gotoAndStop(value);
					break;
				case "general_state_sn_leadin1":
					general_param2.gotoAndStop(value);
					break;
				case "general_state_sn_leadin2":
					general_param3.gotoAndStop(value);
					break;
				case "general_alarm_sn_24v_lost_power":
					general_param4.gotoAndStop(value);
					break;					
				case "general_alarm_fire_alarm":
					general_param5.gotoAndStop(value);
					break;					
				case "general_alarm_intrusion_alarm":
					general_param6.gotoAndStop(value);
					break;
			}			
			break;		
	}
}
