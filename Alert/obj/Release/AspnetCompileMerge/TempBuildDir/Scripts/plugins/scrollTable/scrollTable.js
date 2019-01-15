$(function(){
	$("#data").scroll(function () {
		var scrollLeft = $(this).scrollLeft();
		$("#tHeader").scrollTo(scrollLeft,0,0);
	});
});
function func_get2(){
	//deletes();
	//checkall();
	$('#checkAll').click(function(){
		if(fcontrol == undefined){
			var fcontrol = '';
		}
		if(fcontrol == 'transfer'){
			$(":checkbox[name='keys[]']").attr('checked',$(this).is(':checked'));
			$('input.searchs, input#id').each(function(){
				var id = $(this).attr('id');
				if(id != 'fromdate' && id != 'todate'){
					$(this).val('');
				}
			});
			$('select.combo').each(function(){
				var id = $(this).attr('id');
				if(id != 'process'){
					$(this).multipleSelect('uncheckAll');
				}
			})
		}
		else{
			$(":checkbox[name='keys[]']").attr('checked',$(this).is(':checked'));
			$('input.searchs, input#id').each(function(){
				var id = $(this).attr('id');
				if(id != 'fromdate' && id != 'todate'){
					$(this).val('');
				}
			});
			$('select.combo').multipleSelect('uncheckAll');
		}
	})
	$(":checkbox[name='keys[]']").click(function(){
	//kiem tra xem co check all chua
		var len = $(":checkbox[name='keys[]']:checked").length;
		var len2 = $(":checkbox[name='keys[]']").length;
		$ch = (len==len2)?true:false;
		$("#checkAll").attr('checked',$ch);
	})
	$('.edit').each(function(e){
		 $(this).click(function () { 
			//con tro toi tr
			var _this = $(this);
			//con tro toi check box
			var _checkbox = _this.find(":checkbox[name='keys[]']");
			var check = _checkbox.is(':checked');
			if(!_this.hasClass('transfer')){
				//uncheck all
				$('.edit').parent().find(":checkbox[name='keys[]']").removeAttr('checked');
			}
			
			//click hang nao thi check vao hang do va uncheck cac hang khac
			//console.log(check);
			if(check == true){
				_checkbox.removeAttr('checked');
			}
			else{
				_checkbox.attr('checked',true);
			}
			//kiem tra xem co check all chua
			var len = $(":checkbox[name='keys[]']:checked").length;
			var len2 = $(":checkbox[name='keys[]']").length;
			$ch = (len==len2)?true:false;
			$("#checkAll").attr('checked',$ch);
		});
	});
}