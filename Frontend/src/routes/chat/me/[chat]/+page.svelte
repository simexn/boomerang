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
    import InfoSidebar from "$lib/components/chat/InfoSidebar.svelte";
    import { goto } from "$app/navigation";

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
    function createChatItem(data: any, userPfp: string, eventType: string): ChatItem {
    const dateObject = new Date();
    const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

    let chatItem: Partial<ChatItem> = {
        id: data.id,
        timestamp: Date.now().toLocaleString(),
        date: dateObject.toLocaleDateString(),
        time: time,
        isEvent: eventType !== 'ReceiveMessage',
        withoutDetails: false,
        userPfp: `${userPfp}?${Date.now()}`,
    };

    if (eventType === 'ReceiveMessage') {
    const lastMessage = chatItems[chatItems.length - 1];
    const lastMessageTime = new Date(lastMessage.date + ' ' + lastMessage.time);
    const newMessageTime = new Date();
    const timeDifference = (newMessageTime.getTime() - lastMessageTime.getTime()) / 60000; // difference in minutes

    chatItem = {
        ...chatItem,
        content: data.text,
        userName: data.fromUser.userName,
        userId: data.fromUser.id,
        isActive: data.fromUser.isActive,
        withoutDetails: lastMessage.userId === data.fromUser.id && timeDifference < 5
    };
} else {
    chatItem = {
        ...chatItem,
        content: eventType,
        userName: data.userName,
        userId: data.id,
        isActive: data.isActive
    };
}

return chatItem as ChatItem;
}

    async function setupConnection() {
        if (connection) {
            await connection.stop();
        }

        connection = new HubConnectionBuilder()
            .withUrl(`${backendUrl}/chatHub`)
            .build();

            connection.on("ReceiveMessage", async function(data: any, userPfp: string){
    
                let chatItemToAdd = createChatItem(data, userPfp, 'ReceiveMessage');

                chatItems = [...chatItems, chatItemToAdd];

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
</script>

<div class="chat-container d-flex flex-column container-fluid">   
     <ChatHeader bind:isInfoSidebarOpen {groupInfo} {friendInfo} {isUsersSidebarOpen} {userInfo} {chatId}  />
    <ChatMessage {groupInfo} {chatId} {chatItems} {userInfo} {imageUrl} bind:scrollContainer/>  
    {#if !groupInfo?.isArchieved}
    <div class="chat-footer" style="min-height:6rem; height:6rem;" >
        <ul class="list-unstyled">
            <li class="bg-white mb-3">
                <div class="d-flex">
                    <input class="form-control" bind:value={sendMessageText} placeholder="Message" id="textAreaExample2"/>
                    <button type="button" class="btn btn-info btn-rounded float-end" on:click={async () => await sendMessage()}>Send</button>
                </div>
            </li> 
        </ul>
    </div>  
    {/if}
    {#if isInfoSidebarOpen}
        <InfoSidebar bind:isInfoSidebarOpen {groupInfo} {friendInfo} {userInfo} {imageUrl} {chatId}/>
    {/if}
</div>

<style>
    .chat-container{
        padding: 0;
        height:100%;
        box-sizing: border-box;
    }
    .chat-footer{
        margin-left: 5px;
        margin-right:5px;
    } 
</style>