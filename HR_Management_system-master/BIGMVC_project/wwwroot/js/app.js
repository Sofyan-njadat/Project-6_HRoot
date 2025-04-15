$('.show-sidebar-btn').click(function(){
    $('.sidebar').animate({marginLeft:"0px"})
})
$('.hide-sidebar-btn').click(function(){
    $('.sidebar').animate({marginLeft:"-500px"})
})

$(".full-screen-btn").click(function(){
    let current =$(this).closest(".card");
    current.toggleClass("full-screen");
    if(current.hasClass("full-screen")){
        $(this).html("<i class='feather-minimize-2'></i>")
    }else{
        $(this).html("<i class='feather-maximize-2'></i>")
    }
})

const screenHeight = $(window).height();
const currentMenuHeight = $(".nav-menu .active").offset().top;

if(currentMenuHeight > screenHeight){
    $(".sidebar").animate({
        scrollTop:currentMenuHeight-100
    },1000)
}
