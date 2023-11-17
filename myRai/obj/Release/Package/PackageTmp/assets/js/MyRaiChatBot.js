
function ChatShowBox() {
    event.preventDefault();

    //Button Show Chat
    $('#chatBoxButton').removeClass('chatBtnFadeIn');
    $('#chatBoxButton').addClass('chatBtnFadeOut');
    //Chat container
    $('#chatBoxContainer').addClass('chatFadeIn');
    $('#chatBoxContainer').removeClass('chatFadeOut');
    //Button Hide Chat
    $('#chatCloseBoxButton').addClass('chatBtnFadeIn');
    $('#chatCloseBoxButton').removeClass('chatBtnFadeOut');

    ChatSend('/help');

    $('#chatMsg').focus();
}
function ChatHideBox() {
    event.preventDefault();

    //$('#chatBoxContainer').hide();
    $('#chatBoxContainer').removeClass('chatFadeIn');
    $('#chatBoxContainer').addClass('chatFadeOut');
    //$('#chatBoxButton').show();
    $('#chatBoxButton').addClass('chatBtnFadeIn');
    $('#chatBoxButton').removeClass('chatBtnFadeOut');
    //
    $('#chatCloseBoxButton').removeClass('chatBtnFadeIn');
    $('#chatCloseBoxButton').addClass('chatBtnFadeOut');
}

function ChatSendMessage() {
    var chatMsg = $('#chatMsg').val();

    if (chatMsg != '') {
        ChatSendAnything(chatMsg);
    }
}
function ChatSendCommand(command) {
    ChatSendAnything("/" + command);
}
function ChatSendAnything(anything) {

    $('#chatContainer').append('<div class="chat-line"><div class="chat-msg chat-msg-sent bg-primary"><span>' + anything + '</span></div></div>');
    $('#chatMsg').val('');
    $('#chatMsg').focus();
    ChatAutosize();

    ChatSend(anything);
}

function ChatSend(anything) {

    $('#chatContainer').append('<div id="chatWaitLine" class="chat-line"><div class="chat-msg chat-msg-rec"><i class="appendMovingDots">sta scrivendo</span></div></div>');
    $("#chatContainer").scrollTop($("#chatContainer")[0].scrollHeight);

    setTimeout(function () {
        $.ajax({
        url: '/ChatBot/SendMessage',
        type: 'POST',
        dataType: 'json',
        data: { message: anything },
        success: function (data) {

                $('#chatWaitLine').remove();

            var retMsg = data.message;
            switch (data.typeMessage) {
                case "simple":
                    $('#chatContainer').append('<div class="chat-line"><div class="chat-msg chat-msg-rec"><span>' + retMsg + '</span></div></div>');
                    $("#chatContainer").scrollTop($("#chatContainer")[0].scrollHeight);
                    break;
                case "commandList":
                    var toAppend = '';
                    if (retMsg != '') {
                        toAppend = '<div class="chat-line"><div class="chat-msg chat-msg-rec"><span>' + retMsg + '</span></div></div>';
                    }

                    toAppend += '<div class="chat-line"><div class="chat-commands-list"><div class="chat-command-list-internal">';
                    for (var i = 0; i < data.commands.length; i++) {
                        toAppend += '<a href="#" class="btn btn-primary chat-command" onclick="ChatSendCommand(\'' + data.commands[i].Item1 + '\')">';
                        if (data.commands[i].Item3 != '') {
                            toAppend += '<i class="' + data.commands[i].Item3 + '">&nbsp;</i>';
                        }
                        toAppend += data.commands[i].Item2 + '</a>';
                    }
                    toAppend += '</div></div></div>';

                    $('#chatContainer').append(toAppend);
                    $("#chatContainer").scrollTop($("#chatContainer")[0].scrollHeight);
                default:
                    break;
            }


        }
        });

    }, 1000);
}


function ChatAutosize() {
    setTimeout(function () {
        $('#chatMsg').css('height', '0px');

        var scrollheight = $('#chatMsg')[0].scrollHeight;
        var height = Math.min(22 * 5, scrollheight);
        if (height < 25) {
            height = 25;
        }
        $('#chat-msg-sender').css('height', (height + 14) + 'px');
        $('#chatMsg').css('height', height + 'px');
    }, 0);

    if (event.location == 0 && event.keyCode == 13) {
        event.preventDefault();
        ChatSendMessage();
    }
}




