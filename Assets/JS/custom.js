$(function ()
{
    if ($(window).width() < 992) 
    {
        $('.navbar').addClass('active');
    }
    else 
    {
        $('.navbar').removeClass('active');
    }

    $(window).on('scroll', function() 
    {
        if($(window).scrollTop() > 10) 
        {
            $('.navbar').addClass('active');
        }
        else
        {
            if($(window).width() > 992)
            {
                $('.navbar').removeClass('active');
            } 
        }
    });

    $(window).on('resize', function()
    {
        if ($(window).width() < 992) 
        {
            $('.navbar').addClass('active');
        }
        else 
        {
            $('.navbar').removeClass('active');
        }
    });

});


$(function(){ 
    var navMain = $(".navbar-collapse"); 
    navMain.on("click", "a:not([data-toggle])", null, function () {
        navMain.collapse('hide');
    });
});