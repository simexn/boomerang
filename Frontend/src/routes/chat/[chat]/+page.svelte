<script lang="ts">
    import { page } from '$app/stores';
    import { handleMessageSubmit, fetchMessages } from "$lib/Handlers/chatHandler";
    import { handleLeaveGroup, handleDeleteGroup } from "$lib/Handlers/groupHandler";
    import {getToken} from "$lib/Handlers/authHandler"; 
    import { onMount } from 'svelte';
    import { HubConnectionBuilder } from '@microsoft/signalr';
    import { fetchGroupInfo } from '$lib/Handlers/groupHandler';
    import { fetchUserInfo } from '$lib/Handlers/accountHandler';

    import type { User } from "$lib/Handlers/accountHandler";
    import type { Group } from "$lib/Handlers/groupHandler";
    import type { Message } from "$lib/Handlers/chatHandler";

    let userInfo: User | null;
    let groupInfo: Group | null;
    let messages: Message[] = [];

    let sendMessageText: string;
    let chatId: string;
    let isCreator: boolean;
    
    let message: any;
    let _connectionId: string;
    let connection: any;

    $: chatId = $page.params.chat;
    $: chatId && setupConnection();

    onMount(async () => {
        await getUserInfo();
        await getGroupInfo();
        await loadMessages();

        if(userInfo && groupInfo && userInfo.id == groupInfo.creatorId) {
            isCreator = true;
        }
    });

    async function loadMessages() {
        messages = await fetchMessages(chatId);
    }
    async function setupConnection() {
        if (connection) {
            await connection.stop();
        }

        

        connection = new HubConnectionBuilder()
            .withUrl("https://localhost:5000/chatHub")
            .build();

        connection.on("ReceiveMessage", function(data: any){
            let messageToAdd:Message = {
                id: data.id,
                message: data.text,
                chatId: data.chatId,
                fromUser: data.fromUser.userName,
                date: new Date(data.timestamp).toLocaleString()
            };

            messages = [...messages, messageToAdd];
        });

        await connection.start()
            .then(function(){
                connection.invoke('getConnectionId')
                .then(function(connectionId: string){
                    _connectionId = connectionId;
                    
                    joinRoom();
                })
            })
            .catch(function(err: any){
                console.error(err.toString());
            });
    }

    async function joinRoom() {
        let token = await getToken();

        const response = await fetch('https://localhost:5000/chat/joinRoom', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                connectionId: _connectionId,
                roomName: chatId
            })
        });

        if (!response.ok) {
            console.error('Error joining room:', await response.text());
        } 
    }

    async function getUserInfo(){
        userInfo = await fetchUserInfo();
    }
    async function getGroupInfo(){       
            groupInfo = await fetchGroupInfo(chatId);        
    }


    async function leaveGroup(){
        await handleLeaveGroup(chatId);
        window.location.href = '/chat/home';

    }
    

    async function deleteGroup(){
        if (isCreator) {
            await handleDeleteGroup(chatId);
            window.location.href = '/chat/home';
        }
    }

    async function sendMessage() {
        message = await handleMessageSubmit(sendMessageText, chatId, chatId);
        sendMessageText = "";
    }
</script>



<div class="chat">    
    <div class="chat-header">
        <button on:click={async () => await leaveGroup()}>Leave Group</button>
        {#if isCreator} <!-- render the delete button only if the user is the group creator -->
            <button on:click={async () => await deleteGroup()}>Delete Group</button>
        {/if}
    </div>
    <div class="chat-body">
        {#if messages.length != 0}
            {#each messages as message (message.id)}
                <div class="message">
                    <header>{message.fromUser}</header>
                    <p>{message.message}</p>
                    <footer>{message.date}</footer>
                </div>
            {/each}
        {/if}
    </div>
    <div class="post-create__container AdvancedTextEditor__ctr" id="post-create">
        <form id="create_post" class="">
            <div class="AdvancedTextEditor">
                <div class="AdvancedTextEditor__body">
                    <div role="application" id="centerChannelFooter" aria-label="message input complimentary region"
                        tabindex="-1" class="AdvancedTextEditor__cell a11y__region">
                        <div class="textarea-wrapper">
                            <div class="">
                                <div aria-live="polite" role="alert" class="sr-only"></div>
                                <div>
                                    <div>
                                        <div class="form-control custom-textarea custom-textarea--emoji-picker"
                                            spellcheck="true" data-testid="post_textbox_placeholder"
                                            style="overflow: hidden; text-overflow: ellipsis; opacity: 0.5; pointer-events: none; position: absolute; white-space: nowrap; background: none; border-color: transparent;">
                                            Write to Town Square</div><textarea data-testid="post_textbox"
                                            id="post_textbox" autocomplete="off"
                                            class="form-control custom-textarea custom-textarea--emoji-picker"
                                            spellcheck="true" role="textbox" aria-label="write to town square"
                                            dir="auto" style="visibility: visible; height: 46px;"></textarea>
                                        <div style="height: 0px; overflow: hidden;"><textarea
                                                id="post_textbox-reference" dir="auto" rows="1" autocomplete="off"
                                                class="form-control custom-textarea custom-textarea--emoji-picker"
                                                spellcheck="true" aria-hidden="true"
                                                style="visibility: visible;"></textarea></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                </div>
                <div id="postCreateFooter" role="form" class="AdvancedTextEditor__footer"><span
                        class="msg-typing"></span></div>
            </div>
        </form>
    </div>
    </div>

<style>
    .chat{
        flex-grow: 1;
        display: flex;
        flex-direction: column;
        max-height: 100%;
    }
    .chat-header{
        background-color: var(--bg);
        display: flex;
        justify-content: flex-end;
        padding: 1rem;
    }
    .chat-body{
        background-color: var(--bg);
        flex-grow: 1;
        display: flex;
        flex-direction: column;

        padding-bottom: 1rem;
        overflow: auto;
    }
    
    .chat-footer{
        background-color: var(--bg);
        min-height: 115px;
    }
    .message-input{
        z-index: 12;
    width: 100%;
    flex: 0 0 auto;
    background: var(--center-channel-bg);
    }

    .message{
        display: flex;
        flex-direction: row;
        margin-top:0.2rem;
    }

    .message:first-child{
        margin-top: auto;
    }

    .message > *{
        padding:0.2rem;
    }

    .message > header{
        font-weight: bold;
        min-width: 100px;
        text-align: right;
    }

    .message > p{
        margin:0;
        flex-grow:5;
    }

    .message > footer{
        min-width:60px;
        text-align: center;
    }

</style>