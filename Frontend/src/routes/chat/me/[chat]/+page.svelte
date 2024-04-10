<script lang="ts">
    import { page } from "$app/stores";
    import { fetchGroupInfo, type Group } from "$lib/Handlers/groupHandler";
    import ChatHeader from "$lib/components/chat/ChatHeader.svelte";
    import { onMount, tick } from "svelte";
    import AddFriendModal from "../../AddFriendModal.svelte";
    import { fetchMessages, type ChatItem, handleMessageSubmit } from "$lib/Handlers/chatHandler";
    import type { User } from "$lib/Handlers/accountHandler";
    import { userStore } from "$lib/stores/userInfoStore";
    import ChatMessage from "$lib/components/chat/ChatMessage.svelte";
    import { HubConnectionBuilder, type HubConnection } from "@microsoft/signalr";
    import { getToken } from "$lib/Handlers/authHandler";
    import { fetchFriendInfo, type FriendInfo } from "$lib/Handlers/userHandler";
    import FriendInfoSidebar from "$lib/components/chat/FriendInfoSidebar.svelte";
    import { goto } from "$app/navigation";
    import { json } from "@sveltejs/kit";

    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let chatId: string;
    let groupInfo:Group;
    let isUsersSidebarOpen:boolean = false;
    let isInfoSidebarOpen:boolean = false;
    let isCreator:boolean = false;
    let chatItems: ChatItem[] = [];
    let userInfo: User;
    let friendInfo: FriendInfo;
    let scrollContainer:any;
    let message:any;
    let connection: HubConnection;
    let _connectionId: string;
    let sendMessageText = '';

    let isPickerOpen = false;
    let EmojiPicker: any;

    let ready = false;

    let imageUrl = '/user-icon-placeholder.png';

    $: chatId = $page.params.chat;
    $: chatId && setupConnection();

    userStore.subscribe(value => {
        userInfo = value;
    });
    
    onMount(async() => {
        console.log(chatId);
        await getGroupInfo();
        await getFriendInfo();
        await loadMessages();
        
        if (typeof window !== 'undefined') {
            const module = await import('emoji-picker-element');
            EmojiPicker = module.default;
        }
        ready = true;
    });
    async function getGroupInfo(){
        groupInfo = await fetchGroupInfo(chatId);
    }
    async function getFriendInfo(){
        friendInfo = await fetchFriendInfo(chatId);
    }
    async function loadMessages(){
        chatItems = await fetchMessages(chatId); 
    }
    async function sendMessage() {
        message = await handleMessageSubmit(sendMessageText, chatId);
        sendMessageText = "";
    }
    function createChatItem(data: any, eventType: string): ChatItem {
    const dateObject = new Date();
    const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

    let chatItem: Partial<ChatItem> = {
        id: data.message.id,
        timestamp: Date.now().toLocaleString(),
        date: dateObject.toLocaleDateString(),
        time: time,
        isEvent: eventType !== 'ReceiveMessage',
        withoutDetails: false,
        userPfp: `${backendUrl}${data.userPfp}`,
    };
    if (eventType === 'ReceiveMessage') {
        if (chatItems.length > 0) {
        const lastMessage = chatItems[chatItems.length - 1];
        const lastMessageTime = new Date(lastMessage.date + ' ' + lastMessage.time);
        const newMessageTime = new Date();
        const timeDifference = (newMessageTime.getTime() - lastMessageTime.getTime()) / 60000; // difference in minutes

        chatItem = {
            ...chatItem,
            content: data.message.text,
            userName: data.message.fromUser.userName,
            userId: data.message.fromUser.id,
            isActive: data.message.fromUser.isActive,
            withoutDetails: lastMessage.userId === data.message.fromUser.id && timeDifference < 5
        };
    } else {
        chatItem = {
            ...chatItem,
            content: data.message.text,
            userName: data.message.fromUser.userName,
            userId: data.message.fromUser.id,
            isActive: data.message.fromUser.isActive,
            withoutDetails: false
        };
    }
} else {
    chatItem = {
        ...chatItem,
        content: eventType,
        userName: data.message.userName,
        userId: data.message.id,
        isActive: data.message.isActive
    };
}

return chatItem as ChatItem;
}

    async function setupConnection() {
        if (connection) {
            ready=false;
            await connection.stop();
        }
        

        connection = new HubConnectionBuilder()
            .withUrl(`${backendUrl}/chatHub`)
            .build();

            connection.on("ReceiveMessage", async function(data: any){
                
                console.log("message received")
                console.log(data);

               try {
                    let chatItemToAdd = createChatItem(data, 'ReceiveMessage');
                    console.log(chatItemToAdd);
                    chatItems = [...chatItems, chatItemToAdd];
                } catch (error) {
                    console.error('Error creating chat item:', error);
                }

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });

        connection.on("EditedMessage", async function(data: any){
            chatItems = chatItems.map(item => item.id === data.id ? {...item, content: data.text, isEdited:true} : item);
        });
        connection.on("DeletedMessage", async function(data: any){
            chatItems = chatItems.map(item => item.id === data.id ? {...item, isDeleted:true} : item);
        });
        connection.on("FriendRemoved", async function(){
            console.log("friendremove accessed")
            await goto('/chat/home');
        });
        
        await connection.start()
            .then(function(){
                connection.invoke('getConnectionId')
                .then(async function(connectionId: string){
                    _connectionId = connectionId;
                    await loadMessages();
                    await getGroupInfo();
                    await getFriendInfo();
                    await joinRoom();
                    await tick();
                    ready=true;
                    
                })
            })
            .catch(function(err: any){
                console.error(err.toString());
            });
    }

    async function joinRoom() {
        let token = await getToken();

        const response = await fetch(`${backendUrl}/chat/joinRoom`, {
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
    function addEmoji(event: any) {
    sendMessageText += event?.detail?.unicode;
    isPickerOpen = false;
  }

  function togglePicker() {
    isPickerOpen = !isPickerOpen;
  }
</script>
{#if ready}
<div class="chat-container d-flex flex-column container-fluid">   
    <ChatHeader bind:isInfoSidebarOpen {groupInfo} {friendInfo} {isUsersSidebarOpen} {userInfo} {chatId}  />
    <ChatMessage {groupInfo} {chatId} {chatItems} {userInfo} {imageUrl} bind:scrollContainer/>  
    <div class="chat-footer" style="min-height:6rem; height:6rem;" >
        <div class="send-message-wrap">
            <div class="textarea-container">
                <textarea placeholder="Type a message..." bind:value={sendMessageText} on:keydown={e => e.key === 'Enter' && sendMessage()}></textarea>
            </div>
            <div class="message-options">
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <div class="message-option emoji" on:click={togglePicker}>
                    <i class="fa-regular fa-face-smile"></i>
                    {#if isPickerOpen}
                    <emoji-picker class="light" on:emoji-click={addEmoji}></emoji-picker>
                    {/if}
                </div>
                
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <div class="send-message-button" class:disabled={sendMessageText == null || sendMessageText == ""} on:click={sendMessage}>
                    
                    <i class="material-icons" class:disabled={sendMessageText == null || sendMessageText == ""} >send</i>
                </div>
            </div>
        </div>
    </div>  
    {#if isInfoSidebarOpen}
        <FriendInfoSidebar bind:isInfoSidebarOpen {groupInfo} {friendInfo} {userInfo} {imageUrl} {chatId}/>
    {/if}
</div>
{/if}



<style>
    .chat-container{
        padding: 0;
        height:100%;
        box-sizing: border-box;
    }
    .chat-footer{
        margin-left: 1rem;
        margin-right:1rem;
        margin-bottom: 1rem;
    } 
    .send-message-wrap{
        display: flex;
        flex-direction: column;
        border: 2px solid rgba(var(--bg-sec), 0.16);
        border-radius: 0.25rem;
    }
    .textarea-container{
        position: relative;
       
        
        
    }
        .textarea-container textarea{
            width: 100%;
            resize:none;
            display: block;
            width: 100%;
            padding: 0.375rem 0.75rem;
            font-size: 1rem;
            font-weight: 400;
            line-height: 1.5;
            color: #212529;
            background-color: #fff;
            background-clip: padding-box;
            /* border: 1px solid rgba(var(--bg-sec), 0.3); */
            border:none;
            height: 2.6rem;
            /* border-radius: 0.25rem; */
            transition: border-color .15s ease-in-out,box-shadow .15s ease-in-out;
        }
    .message-options{
        display: flex;
        justify-content: space-between;
        align-items: center;
        background-color: rgba(var(--bg-sec), 0.10);
        height:2.5rem;
        padding-left: 0.4rem;
        padding-right:0.4rem;
    }
        .message-option{
            display: flex;
            justify-content: center;
            align-items: center;
            width: 2rem;
            height: 2rem;
            cursor: pointer;
            border-radius: 0.25rem;
        }
            .message-option.emoji{
                position: relative;
            }
                .message-option.emoji emoji-picker{
                    position: absolute;
                    bottom: 3rem;
                    left: 0;
                    z-index: 100;
                }
        .message-option:hover{
            background-color: var(--hover-dark);
        }
        .message-option i{
            color: rgba(var(--bg-sec), 0.65);
        }
        .send-message-button{
            margin: 0.5rem 0.25rem 0.5rem 0.25rem;
            display: flex;
            
            justify-content: center;
            align-items: center;
            background-color: var(--button-bg);
            cursor: pointer;
            width: 3.5rem;
            height: 2rem;
            border-radius: 0.25rem;
        }
            .send-message-button.disabled{
                background: rgba(var(--bg-sec),0.1);
                cursor:default;
            }
        .material-icons{
            color: #fff;
        }
            .material-icons.disabled{
                color: rgba(var(--bg-sec),0.32);
            }

    @media (max-width: 600px){
        .chat-footer{
            margin-left: 0;
            margin-right:0;
            margin-bottom: 0;
        }
        .send-message-wrap{
            height: 100%;
        }
    }
</style>