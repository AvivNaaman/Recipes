var BreakException = {};
var isStarClicked = false;

window.dataLayer = window.dataLayer || [];
function gtag() { dataLayer.push(arguments); }
gtag('js', new Date());

gtag('config', 'UA-130632144-2');

const urlParams = new URLSearchParams(window.location.search);
//preloading jQuery plugin
$.fn.preload = function () {
    this.each(function () {
        //$('<img/>')[0].src = this;
        $('<img />').attr('src', this).appendTo('body').css('display', 'none');
    });
};

function StyleForTxtArea() {
    var tx = document.getElementsByTagName('textarea');
    for (var i = 0; i < tx.length; i++) {
        tx[i].setAttribute('style', 'height:' + (tx[i].scrollHeight) + 'px;overflow-y:hidden;');
        tx[i].addEventListener("input", OnInput, false);
    }
}

function OnInput() {
    this.style.height = 'auto';
    this.style.height = this.scrollHeight + 'px';
}
$(document).ready(function () {


    $('.dropdown-toggle').click(function () {
        $(this).next('.dropdown-menu').slideToggle(500);
    });
    StyleForTxtArea();

    //preload my tests image
    function preloadImage(url) {
        var img = new Image();
        img.src = url;
    }
    /*function preload(arrayOfImages) {
        $('<img />').attr('src', arrayOfImages).appendTo('body').css('display', 'none');
    }*/
    //preloadImage("https://images.pexels.com/photos/67636/rose-blue-flower-rose-blooms-67636.jpeg");
    //preload("https://images.pexels.com/photos/67636/rose-blue-flower-rose-blooms-67636.jpeg");

    var urlPath = location.pathname.toLowerCase();
    if (urlPath === "" || urlPath === "/") {
        $("li[data-controller='Home'").addClass("active");
    }
    else {
        var controller = urlPath.split('/')[1];
        $("li[data-controller='" + controller + "'").addClass("active");
    }
    //for manage
    if (urlPath.split('/')[1].toLowerCase() === "manage") {
        urlPath = location.pathname.toLowerCase();
        if (urlPath === "/manage" || urlPath === "/manage/") {
            $("li[data-action='index']").addClass("active");
        }
        else {
            var action = urlPath.split('/')[2];
            $("li[data-action='" + controller + "']").addClass("active");
        }
    }
    $("#LoginButton").click(function (e) {
        if ($("#nav_login_user").val()) {
            if ($("#nav_login_pwd").val()) {
                $(this).prop("disabled", true);
                $.ajax({
                    url: '/account/login',
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ LoginUser: $("input[data-form-name='NavLogin']:text").val(), LoginPwd: $("input[data-form-name='NavLogin']:password").val(), RememberMe: $("input[data-form-name='NavLogin']:checkbox").prop("checked") }),
                    dataType: "json",
                    success: function (data) {
                        if (data['Message'] === "SUCCESS") {
                            location.reload();
                        }
                        else {
                            console.log(data);
                            $('#nav_login_pwd').val("");
                            var errorDest = $('#nav_login_pwd_error');
                            errorDest.empty();
                            var ErrorText = "";
                            if (data['Error'].toString().toUpperCase() === "CREDENTIALS") {
                                ErrorText = 'Some credentials were incorrect.';
                                $(this).prop("disabled", true);
                            }
                            else if (data['Error'].toString().toUpperCase() === "INVALIDCHARS") {
                                ErrorText = 'Invalid characters detected.';
                                $(this).prop("disabled", true);
                            }
                            else {
                                ErrorText = '&nbsp;';
                                $(this).prop("disabled", true);
                            }
                            errorDest.append(ErrorText);
                        }
                    },
                    error: function (xhr, textStatus, errorThrown) {
                        alert(textStatus + " " + errorThrown);
                    }
                });
            }
        }
        return false;

    });

    if (location.pathname.toLowerCase() === "/recipes" || location.pathname.toLowerCase() === "/recipes/") {
        if (urlParams.get("q")) {
            $('#search-page-string').val(urlParams.get("q"));
        }
        if (urlParams.get("s")) {
            $('#search-page-sort').val(urlParams.get("s"));
        }
        if (urlParams.get("c")) {
            $('#search-page-cat').val(urlParams.get("c"));
        }
    }
    CheckboxesToggle();
    
    $("body").on('mouseover', 'a', function (e) {
        var $link = $(this),
            href = $link.attr('href') || $link.data("href");

        $link.off('click.chrome');
        $link.on('click.chrome', function () {
            window.location.href = href;
        })
            .attr('data-href', href) //keeps track of the href value
            .css({ cursor: 'pointer' })
            .removeAttr('href'); // <- this is what stops Chrome to display status bar
    });
    //Star rating: OnMouseOver
    $("i[data-form='addComment']").on('mouseover', function (e) {
        if ($("i[data-form='addComment'][data-star='1']").data('submitted') !== true) {
            var $currStar = $(this);
            var starNum = $currStar.data("star");
            var N = 10;
            $("i[data-form='addComment']").each(function () {
                try { $(this).css("color", "black"); } catch (e) { console.write(); }
            });
            var repeatArr = Array.apply(null, { length: starNum + 1 }).map(Number.call, Number);
            repeatArr.forEach(function (e) {
                if (e !== 0) {
                    var starE = $("i[data-form='addComment'][data-star='" + e + "']");
                    starE.css("color", "#ffd800");
                }
            });
        }
        });
    
    $("li[data-form='addComment']").on('mouseover', function () {
        var isOnOne = false;
        if ($("i[data-form='addComment'][data-star='1']").data('submitted') !== true) {
            try {
                $("i[data-form='addComment']").each(function () {
                    if ($(this).is(":hover")) {
                        throw BreakException;
                    }
                });
            }
            catch (e) {
                if (e !== BreakException) throw e;
                else isOnOne = true;
            }
            if (!isOnOne) {
                $("i[data-form='addComment']").each(function () {
                    try { $(this).css("color", "black"); } catch (e) { console.write(); }
                });
            }
        }
    });
    //Star rating: OnClick
    $("i[data-form='addComment']").on('click', function (e) {
        var $currStar = $(this);
        $("i[data-form='addComment']").each(function () {
            try { $(this).css("color", "black"); } catch (e) { console.write(); }
        });
        var starNum = $currStar.data("star");
        var repeatArr = Array.apply(null, { length: starNum + 1 }).map(Number.call, Number);
        repeatArr.forEach(function (e) {
            if (e !== 0) {
                var starE = $("i[data-form='addComment'][data-star='" + e + "']");
                starE.css("color", "#ffd800");
                starE.addClass("checked");
            }
        });
        var score = parseInt($(this).data("star"));
        var recipeRoute = location.pathname.split('/')[3];
        if (score < 6 && score > 0) {
            $.ajax({
                url: "/recipes/rate/",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ Recipe: recipeRoute, Score: score }),
                dataType: "json",
                success: function (data) {
                    $("i[data-form= 'addComment'][data-star='1']").data('submitted', true);
                    console.log("I've sent star rating successfully!");
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + " " + errorThrown);
                }
            });
        }
    });
    var ingsInputCntr = $("input[data-form='edit'][data-field='ingredient']").length;
    $('#edit-form-add-ingredient').on("click", function (e) {
        var $parent = $("#edit-form-ingredients");
        var tagToAdd = $("<input type='text' class='form-control' name='edit-form-ingredient-" + ingsInputCntr + "' id='edit-form-ingredient-" + ingsInputCntr + "' data-form='edit' data-field='ingredient' data-field-ingredient='"+ingsInputCntr+"' required/>");
        tagToAdd.appendTo($parent);
        ingsInputCntr++;
        $('#edit-form-rm-ingredient').prop("disabled", false);
    });
    $('#edit-form-rm-ingredient').on("click", function (e) {
        var inputs = $("input[data-form='edit'][data-field='ingredient']");
        if (inputs.length > 2) {
            inputs.remove(":last");
            ingsInputCntr -= 1;
        }
        if (ingsInputCntr === 2) {
            $(this).prop("disabled", true);
        }
    });
    var instInputCntr = $("textarea[data-form='edit'][data-field='instruction']").length;
    $('#edit-form-add-instruction').on("click", function (e) {
        var $parent = $("#edit-form-instructions");
        var tagToAdd = $("<textarea type='text' class='form-control' name='edit-form-instruction-" + ingsInputCntr + "' id='edit-form-instruction-" + ingsInputCntr + "' data-form='edit' data-field='instruction' data-field-instruction='" + ingsInputCntr + "' required></textarea>");
        tagToAdd.appendTo($parent);
        instInputCntr++;
        $('#edit-form-rm-instruction').prop("disabled", false);
        StyleForTxtArea();
    });
    $('#edit-form-rm-instruction').on("click", function (e) {
        var inputs = $("textarea[data-form='edit'][data-field='instruction']");
        if (inputs.length > 2) {
            inputs.remove(":last");
            instInputCntr -= 1;
        }
        if (instInputCntr === 2) {
            $(this).prop("disabled", true);
        }
    });
    $("#edit-form").submit(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var stdSelector = "[data-form='edit']";

        var timespan = { Ticks: 369000000000, Days: 0, Hours: 10, Milliseconds: 0, Minutes: 15, Seconds: 0, TotalDays: 0.42708333333333331, TotalHours: 10.25, TotalMilliseconds: 36900000, TotalMinutes: 615, TotalSeconds: 36900 };
        var hrs = parseInt($("input" + stdSelector + "[data-field='time'][data-field-time='hr']").val());
        var mins = parseInt($("input" + stdSelector + "[data-field='time'][data-field-time='min']").val());
        timespan.Seconds = 0;
        timespan.Minutes = mins;
        timespan.Hours = hrs;

        timespan = timespan.Hours + ":" + timespan.Minutes + ":00";

        var ingreds = "";
        $("input[data-form='edit'][data-field='ingredient']").each(function (e) {
            if (ingreds !== "") {
                ingreds += ';';
            }
            ingreds += $(this).val();
        });

        var insts = "";
        $("textarea[data-form='edit'][data-field='instruction']").each(function (e) {
            if (insts !== "") {
                insts += ';';
            }
            insts += $(this).val();
        });
        //TODO: Add submission check logic HERE!
        var postData = {
            ID: parseInt(location.pathname.split('/')[3]),
            UserID: 0,
            CategoryID: $("select" + stdSelector + "[data-field='category']").val(),
            Title: $("input" + stdSelector + "[data-field='title']").val(),
            Description: $("textarea" + stdSelector + "[data-field='description']").val(),
            TotalTime: timespan,
            Ingredients: ingreds,
            Instructions: insts,
            publicStatus: $('#edit-form-public-chkbox').prop("checked"),
            RecipeYield: $('input' + stdSelector + '[data-field="yield"]').val(),
            KeyWords: $('input' + stdSelector + '[data-field="kweywords"]').val()
        };

        //TODO: Add Ajax Json
        $.ajax({
            url: "/recipes/edit",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(postData),
            dataType: "json",
            xhr: function () {
                var xhr = $.ajaxSettings.xhr();
                xhr.upload.onprogress = function (e) {
                    if (e.lengthComputable) {
                        console.log(e.loaded / e.total);
                    }
                };
                return xhr;
            },
            success: function (data) {
                console.log(data);
                $("#edit-form-submit").removeClass("btn-primary");
                $("#edit-form-submit").addClass("btn-success");
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
            }
        });
        return false;
    });
    $('input[data-form="edit"], textarea[data-form="edit"], select[data-form="edit"]').click(function () {
        $('#edit-form-submit').removeClass('btn-success').addClass('btn-primary');
    });
    /*if (!(location.href.split('/').length === 2 || location.href.split('/').length === 3)) {
        if (location.href.split('/')[1].toLowerCase() === 'recipes') {*/

    Search(false);
        /*}
    }*/

    $("#reset-pwd-form").submit(function (e) {
        e.preventDefault();
        e.stopPropagation();
        $.ajax({
            url: '/Account/Reset',
			type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ email: $('#reset-pwd-email').val()}),
            dataType: "json",
            success: function (data) {
                $('#reset-pwd-submit').addClass("disabled").removeClass("btn-outline-info").addClass("btn-success");
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
                console.write(xhr.responseText);
            }
        });
        return false;
    });
    var ResPwdSubmitParent = $('#reset-pwd-submit').parent().parent();
    var ResPwdError = $('#reset-pwd-error').empty().append("&nbsp;");
    $("#reset-pwd-form-v").submit(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var pwVal0 = $('#reset-pwd-pwd0').val();
        var pwVal = $('#reset-pwd-pwd').val();
        if (pwVal !== pwVal0) {
            ResPwdError.empty().append("Please provide same passwords.");
        }
        else {
            $('#reset-pwd-error').empty().append("&nbsp;");
            $('#reset-pwd-submit').addClass("disabled");
            $.ajax({
                url: '/Account/ResetV',
                type: "POST",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ pass: pwVal, pass0: pwVal0, id: location.href.split('/')[5] }),
                dataType: "json",
                success: function (data) {
                    var txt = "";
                    if (data.Message === "SUCCESS") {
                        ResPwdError.removeClass("text-danger").addClass("text-success").empty().append("password changed.");
                        location.href = "/";
                    }
                    else if (data.Error === "SameAsOld")
                    {
                        txt = "Don't Use One of your old passwords, please!";
                    }
                    else {
                        txt = "General Error: " + data.Error;
                    }
                    $('#reset-pwd-submit').removeClass("disabled");
                    ResPwdError.empty().append(txt);
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + " " + errorThrown);
                    console.write(xhr.responseText);
                }
            });
        }
        return false;
    });
    $("#recipe-saved-icon").click(function () {
        $.ajax({
            url: '/account/ChangeSavedStatus/' + location.pathname.split('/')[3],
			type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                if (data.Message === "SUCCESS") {
                    console.log("Saved state successfully changed.");
                    toggleImageChecked("#recipe-saved-icon");
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
            }
        });
    });
    $('#contact-form').submit(function (e) {
        e.preventDefault();
        e.stopPropagation();
        $.ajax({
            url: "/about/contact",
					type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ mailAddr: $('input[type="email"][data-form="contact"]').val(), title: $('input[type="text"][data-form="contact"]').val(), body: $('textarea[data-form="contact"]').text() }),
            dataType: "json",
            success: function (data) {
                $('[data-form="contact"]').each(function (e) {
                    $(e).prop("disabled", true);
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
            }
        });
        return false;
    });
    $("#manage-config-form").submit(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var Smtp = {};
        if ($("#manage-config-smtp-enabled").prop("checked")) {
            //then, smtp enabled
            Smtp["User"] = $("#manage-config-smtp-user").val();
            Smtp["Password"] = $("#manage-config-smtp-password").val();
            Smtp["Server"] = $("#manage-config-smtp-server").val();
        }
        $.ajax({
            url: '/manage/index',
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ SmtpEnabled: $("#manage-config-smtp-enabled").prop("checked"), Smtp: Smtp, FontFamily: $("#manage-config-font-family").val(), SiteName: $("#manage-config-website-name").val(), DarkMode: $("#manage-config-dark").prop("checked") }),
            dataType: "json",
            success: function (data) {
                console.log(data);
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
            }
        });
        return false;
    });
    $('#search-page-submit').click(function (e) {
        var queriesToAdd = [];
        if ($("#search-page-cat").val()) {
            queriesToAdd.push("c=" + $("#search-page-cat").val());
        }
        if ($("#search-page-sort").val()) {
            queriesToAdd.push("s=" + $("#search-page-sort").val());
        }
        if ($("#search-page-string").val()) {
            queriesToAdd.push("q=" + $("#search-page-string").val());
        }
            history.pushState(null, "Search", location.pathname + "?" + queriesToAdd.join("&"));
            Search(true);
    });
    $("i[data-field='manage-users-rm']").click(function () {
        if (confirm("Are you sure you'd like to delete this user?")) {
            var UserID = $(this).data("user-id");
            var $RowToDelete = $('table > tbody > tr[data-user-id="' + UserID + '"]');
            $RowToDelete.css("visibillity", "hidden");
            $.ajax({
                url: '/manage/deleteuser/' + UserID,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    console.log(data);
                    if (data.Message === "SUCCESS") {
                        $RowToDelete.remove();
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + " " + errorThrown);
                }
            });
        }
    });
    var lastVal;
    lastVal = $('[data-field="manage-users-role"]').text();
    $('[data-field="manage-users-role"]').focusin(function () {
        lastVal = $(this).text();
    });
    $('[data-field="manage-users-role"]').focusout(function () {
        var $role = $(this).text();
        var userID = $(this).data("user-id");
        var url = '/manage/changerole/' + userID + "?r=" + $role;
        if ($role !== lastVal) {
            $.ajax({
                url: url,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    console.log(data);
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + " " + errorThrown);
                }
            });
        }
    });
    $('[data-field="change-public-status-manage"]').change(function () {
        var url = '/manage/changerecipepublic/' + $(this).parent().parent().data("recipe-id");
        var $chkbx = $(this);
        $.ajax({
            url: url,
					type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                if (data.Message === "SUCCESS") {
                    console.log("Successfully updated public status");
                }
                else if (data.Message === "Error") {
                    if (data.Error === "InfoNotFull") {
                        $chkbx.prop("checked", false);
                        //$chkbx.parent().find("div[data-append-here='true']").empty().append($("<small> Edit to publish</small>"));
                        alert("Publishing Recipe Required Editing And Saving it.")
                    }
                    else {
                        console.log(data);
                    }
                }
                
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
                console.log(xhr.responseText);
            }
        });
    });
    $('[data-field="remove-recipe-manage"]').click(function () {
        var $rowToRemove = $(this).parent().parent();
        var url = '/manage/deleterecipe/' + $rowToRemove.data("recipe-id");
        $.ajax({
            url: url,
					type: "POST",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                console.log(data);
                if (data.Message === "SUCCESS") {
                    $rowToRemove.remove();
                }
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
            }
        });
    });
        $('input[data-form="account"]').on('input', function (e) {
            $('button[data-form="account"][type="submit"]').prop("disabled", false);
        });
    $("#account-button-clear-data").click(function () {
        if (confirm("Are You Sure You'd like to Remove Data?")) {
            $.ajax({
                url: '/account/cleardata',
					type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    $(this).prop("disabled", true);
                    if (data.Message === "SUCCESS") { alert();}
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + " " + errorThrown);
                }
            });
        }
    });
    $("#account-button-delete-account").click(function () {
        if (confirm("Are You Sure You'd like to Delete your account?")) {
            $.ajax({
                url: '/account/delete',
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    $(this).prop("disabled", true);
                    if (data.Message === "SUCCESS") {
                        location.href = "/";
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    alert(textStatus + " " + errorThrown);
                }
            });
        }
    });
    $("#account-form").submit(function (e) {
        e.preventDefault();
        e.stopPropagation();
        $.ajax({
            url: '/account/change',
					type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ ID: 1, FirstName: $(this).children().find("#account-name").val(), UserName: $(this).children().find("#account-user").val(), Email: $(this).children().find("#account-email").val(), BirthDay: $(this).children().find("#account-birth-day").find("input").val()  }),
            dataType: "json",
            success: function (data) {
                if (data.Message === "SUCCESS") {
                    $("#account-submit").prop("disabled", true);
                }
                else { console.log(data);}
            },
            error: function (xhr, textStatus, errorThrown) {
                alert(textStatus + " " + errorThrown);
            }
        });
        return false;
    });
});
function toggleImageChecked(imgSelector) {
    var $img = $(imgSelector);
    var tmpArr = $img.attr("src").split('/');
    if (tmpArr[tmpArr.length - 1].includes("unchecked")) {
        tmpArr[$img.attr("src").split('/').length - 1] = tmpArr[$img.attr("src").split('/').length - 1].replace("unchecked", "checked");
        $img.attr("src", tmpArr.join('/'));
    }
    else {
        tmpArr[$img.attr("src").split('/').length - 1] = tmpArr[tmpArr.length - 1].replace("checked", "unchecked");
        $img.attr("src", tmpArr.join('/'));
    }
}
function SaveFullComment() {
    var ratingTitle = $("#addComment-title").val();
    var ratingComment = $("#addComment-body").val();
    var recipeRoute = location.pathname.split('/')[3];
    $.ajax({
        url: '/recipes/rate/',
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ Recipe: recipeRoute, CommTitle: ratingTitle, CommBody: ratingComment }),
        dataType: "json",
        success: function (data) {
            console.log("I've sent comment successfully!");
        },
        error: function (xhr, textStatus, errorThrown) {
            alert(textStatus + " " + errorThrown);
        }
    });
}
function buildRecipeCardByObject(recipePar) {
    //var tmplt = '<div class="col-md-3 mb-3 col-center" style="cursor:pointer" onclick="location.href = ' + "'%%RouteURL%%'" + '"> <div class="card"> <img class="card-img-top" src="https://images.pexels.com/photos/67636/rose-blue-flower-rose-blooms-67636.jpeg" alt="Card image cap"> <div class="card-body"> <h5 class="card-title">%%title%%</h5> <p class="card-text">%%description%%</p> <div class="card-footer text-muted align-bottom"> <div class=""> <span class="fa fa-clock-o mr-1 ml-1" style="vertical-align: middle"></span><small style="vertical-align: middle">%%time%%</small> <span class="fa fa-th mr-1 ml-1" style="vertical-align: middle"></span><small><a href="~/Recipes?c= %%cat%% " class="btn btn-outline-secondary text-dark btn-sm" style="border: none; padding: 2px 5px 2px 5px;"> %%cat%% </a></small> </div></div> </div> </div> </div>';
    //var outputStr = tmplt.replace('%%title%%', recipePar.Title).replace('%%RouteURL%%', recipePar.RouteURL).replace('%%time%%', recipePar.Time).replace('%%description%%', recipePar.Description);
    //outputStr = outputStr.replace(' %%cat%% ', recipePar.Category);
    var outputStr = '<div class="col-md-3 col-sm-6 col-xs-12 mb-3" style="cursor:pointer" onclick="location.href = ' + "'/Recipes/Recipe/" + recipePar.RouteURL + "'" + '"> <div class="card"> <img class="card-img-top" src="https://images.pexels.com/photos/67636/rose-blue-flower-rose-blooms-67636.jpeg" alt="Card image cap"> <div class="card-body"> <h5 class="card-title">' + recipePar.Title + '</h5> <p class="card-text">' + recipePar.Description + '</p> <div class="card-footer text-muted align-bottom"> <div class=""> <span class="fa fa-clock-o mr-1 ml-1" style="vertical-align: middle"></span><small style="vertical-align: middle">' + recipePar.Time + '</small> <span class="fa fa-th mr-1 ml-1" style="vertical-align: middle"></span><small><a href="~/Recipes?c=' + recipePar.Category + '" class="btn btn-outline-secondary text-dark btn-sm" style="border: none; padding: 2px 5px 2px 5px;">' + recipePar.Category + '</a></small> </div></div> </div> </div> </div>';
    return outputStr;
}
function CheckboxesToggle() {
    $('input[type="checkbox"][data-chkbox-disable="true"]').each(function () {
        var elem = $(this);
        var $currFormName =elem.data("chkbox-disable-type");
        var $selectorPart = '[data-chkbox-disable-type="' + $currFormName + '"]';
        var $toggledElementsCollection = $('input' + $selectorPart + ':not([type="checkbox"]), textarea' + $selectorPart);
        var dis = true;
        if ($(this).prop("checked")) {
            dis = false;
        }
        $toggledElementsCollection.each(function () {
            $(this).prop("disabled", dis).attr("required", !dis);
        });
    });
}
function Search(reshowLoader) {
    if (location.pathname.toLowerCase() === "/recipes" || location.pathname.toLowerCase() === "/recipes/") {
        var searchDiv = $('#search-results');
        if (reshowLoader) {
            searchDiv.empty();
            searchDiv.append("<div class='row'> <img src='/Content/Animations/loading-ms-spinner.svg' height='100' style='margin: auto !important;' class='d-block' /> </div>");
        }
        var url = '/recipes/GetSearchResults';
        url += "?";
        if (location.href.split("?").length !== 1) {
            url += location.href.split("?")[1];
        }
        var appendThis;
        var cntr = 0;
        $.ajax({
            url: url,
            type: "GET",
            contentType: "application/json; charset=utf-8",
            //data: JSON.stringify(),
            dataType: "json",
            success: function (data) {
                console.log(data);
                setTimeout(function () {
                    if (data.SearchResult.length > 0) {
                        data.SearchResult.forEach(function (e) {
                            var elementToAdd = buildRecipeCardByObject(e);
                            if (cntr % 4 === 0 || cntr === data.SearchResult.length - 1) {
                                if (cntr === 0) {
                                    appendThis = "<div class='row'>";
                                    appendThis += elementToAdd;
                                }
                                else if (cntr === data.SearchResult.length - 1) {
                                    appendThis += elementToAdd;
                                    appendThis += "</div>";
                                }
                                else {
                                    appendThis += "<div class='row'>";
                                    appendThis += elementToAdd;
                                    appendThis += "</div>";
                                }
                            }
                            else {
                                appendThis += elementToAdd;
                            }
                            cntr++;
                        });
                        var arrToLoad = [];
                        var $parsed = $($.parseHTML(appendThis));
                        $parsed.find('img').each(function () { arrToLoad.push($(this).attr("src")); });
                        /*var tmpSet = new Set(arrToLoad);
                        arrToLoad = Array.from(tmpSet);*/
                        $(arrToLoad).preload();
                        searchDiv.empty();
                        searchDiv.append(/*"<div id='new-recipes-search-results'>" +*/ appendThis); //+ "</div>");
                    }
                    else {
                        searchDiv.empty();
                        searchDiv.append("<h4 style='vertical-align: '><i class='fa-lg fa fa-times' style='vertical-align: top; margin-top: 3px;color:red; margin-right: 0.3rem;'></i>No Results Found. Try Other Search.</h4>");
                    }
                }, 1000);
            },
            error: function (xhr, textStatus, errorThrown) {
                console.log(xhr);
            }
        });
    }
}
