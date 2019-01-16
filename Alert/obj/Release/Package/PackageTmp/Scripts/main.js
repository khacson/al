$(function(){
	$("#checkAll").click(function() {
		$(":checkbox[name='keys[]']").attr('checked', $('#checkAll').is(':checked'));
	});
});
function getCombo(id){
	var val = $('#'+id).multipleSelect('getSelects');
	if(typeof val === 'object'){
		val = "";
	}
	return val;
}
function getComboText(id){
	var val = $('#'+id).multipleSelect('getSelects','text');
	if(typeof val === 'object'){
		val = "";
	}
	return val;
}
function getSearch(){
	//var str = '';
	 var obj = {};
	$('input.searchs').each(function(){
		var id = $(this).attr('id');
		var val = $.trim($(this).val().trim());
		obj[id] = val;
	});
	$('select.combos').each(function(){
		var id = $(this).attr('id');
		var val = getCombo($(this).attr('id'));
		val = val.replace(/['"]/g, '');
		obj[id] = val;
	});
	return JSON.stringify(obj);
}	 
function getCheckedId(){
	var strId = '';
	$('#gridView').find('input:checked').each(function () {
		var id = $(this).attr('id');
		if(id != 'checkAll'){
			strId += ',' + $(this).attr('id') ;
		}
	});
	return strId.substring(1);
}
/*var config = {
	apiKey: "AIzaSyCRz3bzg513QpjGhuTDEcMrxRiJ-TGx7G8",
	authDomain: "alert-ae0aa.firebaseapp.com",
	databaseURL: "https://alert-ae0aa.firebaseio.com",
	storageBucket: "alert-ae0aa.appspot.com",
	messagingSenderId: "136320630223"
};*/
