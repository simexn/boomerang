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
    import { fly, slide } from 'svelte/transition';
    import { get } from 'svelte/store';
    import { goto } from '$app/navigation';
    import UserSidebar from './UserSidebar.svelte';
    import ChatHeader from './ChatHeader.svelte';
    import ChatMessage from './ChatMessage.svelte';
    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let imageUrl = '/user-icon-placeholder.png'

    let userInfo: User;
    let scrollContainer: any;
    let groupInfo: Group;
    let chatItems: ChatItem[];
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

    onMount(async () => {
        await getUserInfo();
        await getGroupInfo();
        await loadMessages();


        console.log("group info is:" + groupInfo)
        
        if(userInfo && groupInfo && userInfo.id == groupInfo.creatorId) {
            isCreator = true;
        }
        ready = true;
    });

    
    function closeDropdown() {
    userSidebarDropdown = null;
  }

    async function loadMessages() {
        chatItems = await fetchMessages(chatId);
    }
    async function setupConnection() {
        if (connection) {
            await connection.stop();
        }

        

        connection = new HubConnectionBuilder()
            .withUrl(`${backendUrl}/chatHub`)
            .build();

            connection.on("ReceiveMessage", async function(data: any){
    
                const dateObject = new Date();
                const date = dateObject.toLocaleDateString();
                const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

                let chatItemToAdd:ChatItem = {
                    id: data.id,
                    content: data.text,
                    timestamp: Date.now().toLocaleString(),
                    date: date,
                    time: time,
                    userName: data.fromUser.userName
                };

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });

            connection.on("UserJoined", async function(user: any) {
                groupInfo.users = [...groupInfo.users, user];

                groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

                const dateObject = new Date();
                const date = dateObject.toLocaleDateString();
                const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

                let chatItemToAdd:ChatItem = {
                    id: Date.now(), // Use the current timestamp as a temporary ID
                    content: 'UserJoined',
                    timestamp: Date.now().toLocaleString(),
                    date: date,
                    time: time,
                    userName: user.userName
                };

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });


        connection.on("UserLeft", async function(user: any) {
            groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

            const dateObject = new Date();
            const date = dateObject.toLocaleDateString();
            const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

            let chatItemToAdd:ChatItem = {
                id: Date.now(), // Use the current timestamp as a temporary ID
                content: 'UserLeft',
                timestamp: Date.now().toLocaleString(),
                date: date,
                time: time,
                userName: user.userName
            };

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserKicked", async function(user: any) {
            if (user.id === userInfo.id) {
                goto('/chat/home');
                
            }
            groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);

            const dateObject = new Date();
            const date = dateObject.toLocaleDateString();
            const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

            let chatItemToAdd:ChatItem = {
                id: Date.now(), // Use the current timestamp as a temporary ID
                content: 'UserKicked',
                timestamp: Date.now().toLocaleString(),
                date: date,
                time: time,
                userName: user.userName
            };

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

            const dateObject = new Date();
            const date = dateObject.toLocaleDateString();
            const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

            let chatItemToAdd:ChatItem = {
                id: Date.now(), // Use the current timestamp as a temporary ID
                content: 'UserPromoted',
                timestamp: Date.now().toLocaleString(),
                date: date,
                time: time,
                userName: user.userName
            };

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserDemoted", async function(user: any){
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

            const dateObject = new Date();
            const date = dateObject.toLocaleDateString();
            const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

            let chatItemToAdd:ChatItem = {
                id: Date.now(), // Use the current timestamp as a temporary ID
                content: 'UserDemoted',
                timestamp: Date.now().toLocaleString(),
                date: date,
                time: time,
                userName: user.userName
            };

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("OwnershipTransferred", async function(user: any){
            groupInfo.creatorId = user.id;
            groupInfo.admins = [...groupInfo.admins, user];

            const dateObject = new Date();
            const date = dateObject.toLocaleDateString();
            const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

            let chatItemToAdd:ChatItem = {
                id: Date.now(), // Use the current timestamp as a temporary ID
                content: 'OwnershipTransferred',
                timestamp: Date.now().toLocaleString(),
                date: date,
                time: time,
                userName: user.userName
            };

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
        message = await handleMessageSubmit(sendMessageText, chatId, chatId);
        sendMessageText = "";
    }
    
    
</script>


<svelte:window on:click={closeDropdown} />
{#if ready}
<div class="chat-container d-flex flex-column container-fluid">
    <div style="height: 8%;">    
        <ChatHeader {groupInfo} bind:isUsersSidebarOpen {isCreator} {chatId} />
    </div>
    <div id="chatBody" class="d-flex flex-column" style="height:87.4%;">
        <div>            
            {#if isUsersSidebarOpen}
                <UserSidebar {groupInfo} {userSidebarDropdown} {userInfo} {imageUrl} {chatId}/>
            {/if}           
            <div>   
                <ChatMessage {chatId} {chatItems} {userInfo} {imageUrl} bind:scrollContainer/>
            </div>            
        </div>   
    </div>
    <div class="chat-footer" style="height:6.6%;max-width: {isUsersSidebarOpen ? 'calc(100% - 300px)' : '100%'};" >
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
        
    }
    .chat-body-container{
    flex-grow: 1;
    display: flex;
    overflow: auto; /* add scrolling if the content overflows */
}






    
.chat-footer{
    overflow-y: auto; 
    margin-left: 5px;
    margin-right:5px;
}



 
</style>