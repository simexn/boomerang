<script lang="ts">
    import { page } from '$app/stores';
    import { tick } from 'svelte';
    import { handleMessageSubmit, fetchMessages } from "$lib/Handlers/chatHandler";
    import { handleLeaveGroup, handleDeleteGroup, handleKickUser, handlePromoteUser, handleDemoteUser, handleTransferOwnership } from "$lib/Handlers/groupHandler";
    import {getToken} from "$lib/Handlers/authHandler"; 
    import { onMount } from 'svelte';
    import { HubConnectionBuilder } from '@microsoft/signalr';
    import { fetchGroupInfo} from '$lib/Handlers/groupHandler';
    import { fetchUserInfo } from '$lib/Handlers/accountHandler';
    

    import type { User } from "$lib/Handlers/accountHandler";
    import type { Group } from "$lib/Handlers/groupHandler";
    import type { Message } from "$lib/Handlers/chatHandler";
    import type {ChatItem} from "$lib/Handlers/chatHandler";
    import { userStore, updateUserInfo } from '$lib/stores/userInfoStore';
    import { goto } from '$app/navigation';
    import UserSidebar from '$lib/components/chat/UserSidebar.svelte';
    import ChatHeader from '$lib/components/chat/ChatHeader.svelte';
    import ChatMessage from '$lib/components/chat/ChatMessage.svelte';
    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let imageUrl = '/user-icon-placeholder.png'

    let userInfo: User;
    let scrollContainer: any;
    let groupInfo: Group;
    let chatItems: ChatItem[] = [];
    let chatUsers: User[]= [];
    let isUsersSidebarOpen = false;
    let userSidebarDropdown : any = null;

    let sendMessageText: string;
    let chatId: string;
    let isCreator: boolean;
    
    let message: any;
    let _connectionId: string;
    let connection: any;
    let ready = false;

    $: chatId = $page.params.chat;
    $: chatId && setupConnection();

    userStore.subscribe(value => {
        userInfo = value;
    });

    onMount(async () => {
        await getGroupInfo();
        await loadMessages();
        
        if(userInfo && groupInfo && userInfo.id == groupInfo.creatorId) {
            isCreator = true;
        }
        ready = true;
    });

    
    

    async function loadMessages() {
        chatItems = await fetchMessages(chatId);
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
    const lastMessage = chatItems[chatItems.length - 1];
    const lastMessageTime = new Date(lastMessage.date + ' ' + lastMessage.time);
    const newMessageTime = new Date();
    const timeDifference = (newMessageTime.getTime() - lastMessageTime.getTime()) / 60000; // difference in minutes

    chatItem = {
        ...chatItem,
        content: data.message.text,
        userName: data.message.fromUser.userName,
        userId: data.message.fromUserId,
        isActive: data.message.fromUser.isActive,
        withoutDetails: lastMessage.userId === data.message.fromUser.id && timeDifference < 5
    };
    console.log(chatItem)
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
            await connection.stop();
        }

        connection = new HubConnectionBuilder()
            .withUrl(`${backendUrl}/chatHub`)
            .build();

            connection.on("ReceiveMessage", async function(data: any){
    
                let chatItemToAdd = createChatItem(data, 'ReceiveMessage');
                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });

            connection.on("UserJoined", async function(user: any) {
                groupInfo.users = [...groupInfo.users, user];

                groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

                let chatItemToAdd = createChatItem(user, 'UserJoined');

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });


        connection.on("UserLeft", async function(user: any) {
            groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

            let chatItemToAdd = createChatItem(user, 'UserLeft');

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserKicked", async function(user: any) {
            if (user.id === userInfo.id) {
                goto('/chat/home');
                
            }
            groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);

            let chatItemToAdd = createChatItem(user, 'UserKicked');

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });
        
        connection.on("UserBanned", async function(user: any){
            if (user.id === userInfo.id) {
                goto('/some-other-page');
            }
        });

        connection.on("UserPromoted", async function(user: any){
            groupInfo.admins = [...groupInfo.admins, user];

            let chatItemToAdd = createChatItem(user, 'UserPromoted');

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserDemoted", async function(user: any){
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

            let chatItemToAdd = createChatItem(user, 'UserDemoted');

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("OwnershipTransferred", async function(user: any){
            console.log("Ownership transferred to: ", user);
            groupInfo = { ...groupInfo, creatorId: user.id, admins: [...groupInfo.admins, user] };

            let chatItemToAdd = createChatItem(user, 'OwnershipTransferred');

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
        
        await connection.start()
            .then(function(){
                connection.invoke('getConnectionId')
                .then(async function(connectionId: string){
                    _connectionId = connectionId;
                    await loadMessages();
                    await getGroupInfo();
                    await joinRoom();
                    await tick();
                    scrollContainer.scrollTop = scrollContainer.scrollHeight;
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
    async function getUserInfo(){
        userInfo = await fetchUserInfo();
        
    }
    async function getGroupInfo(){       
            groupInfo = await fetchGroupInfo(chatId);        
    }
    async function sendMessage() {
        message = await handleMessageSubmit(sendMessageText, chatId);
        sendMessageText = "";
    }
    
    
</script>




{#if ready}
<div class="chat-container d-flex flex-column container-fluid">   
    <ChatHeader  bind:groupInfo bind:isUsersSidebarOpen {userInfo} {chatId} />       
    {#if isUsersSidebarOpen}
        <UserSidebar bind:isUsersSidebarOpen {groupInfo} {userSidebarDropdown} {userInfo} {imageUrl} {chatId}/>
    {/if}           
    <ChatMessage {groupInfo} {chatId} {chatItems} {userInfo} {imageUrl} bind:scrollContainer/>  
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
</div>
{/if}



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