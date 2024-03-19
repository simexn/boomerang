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
    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let imageUrl = '/user-icon-placeholder.png'

    let userInfo: User;
    let scrollContainer: any;
    let groupInfo: Group;
    let chatItems: ChatItem[] = [];
    let chatUsers: User[]= [];
    let isUsersSidebarOpen = false;
    let isUserHovered = false;
    let userSidebarDropdown : any = null;

    let sendMessageText: string;
    let chatId: string;
    let isCreator: boolean;
    
    let message: any;
    let _connectionId: string;
    let connection: any;

    $: chatId = $page.params.chat;
    $: chatId && setupConnection();

    onMount(async () => {
        // await getUserInfo();
        // await getGroupInfo();
        // await loadMessages();

        console.log("group info is:" + groupInfo)
        
        if(userInfo && groupInfo && userInfo.id == groupInfo.creatorId) {
            isCreator = true;
        }
    });

    function openDropdown(userId : any, event: any) {
        event.stopPropagation();
        userSidebarDropdown = userSidebarDropdown === userId ? null : userId;
    }
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
    
                let chatItemToAdd:ChatItem = {
                    id: data.id,
                    content: data.text,
                    timestamp: new Date(data.timestamp).toLocaleString(),
                    userName: data.fromUser.userName
                };

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });

            connection.on("UserJoined", async function(user: any) {
                groupInfo.users = [...groupInfo.users, user];

                groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

                let chatItemToAdd:ChatItem = {
                    id: Date.now(), // Use the current timestamp as a temporary ID
                    content: 'UserJoined',
                    timestamp: new Date().toLocaleString(),
                    userName: user.userName
                };

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });


        connection.on("UserLeft", async function(user: any) {
            groupInfo.users = groupInfo.users.filter(u => u.id !== user.id);
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

            let chatItemToAdd:ChatItem = {
                    id: Date.now(), // Use the current timestamp as a temporary ID
                    content: 'UserLeft',
                    timestamp: new Date().toLocaleString(),
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

            let chatItemToAdd:ChatItem = {
                    id: Date.now(), // Use the current timestamp as a temporary ID
                    content: 'UserKicked',
                    timestamp: new Date().toLocaleString(),
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

            let chatItemToAdd:ChatItem = {
                id: Date.now(),
                content: 'UserPromoted',
                timestamp: new Date().toLocaleString(),
                userName: user.userName
            }

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserDemoted", async function(user: any){
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== user.id);

            let chatItemToAdd:ChatItem = {
                id: Date.now(),
                content: 'UserDemoted',
                timestamp: new Date().toLocaleString(),
                userName: user.userName
            }

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("OwnershipTransferred", async function(user: any){
            groupInfo.creatorId = user.id;
            groupInfo.admins = [...groupInfo.admins, user];

            let chatItemToAdd:ChatItem = {
                id: Date.now(),
                content: 'OwnershipTransferred',
                timestamp: new Date().toLocaleString(),
                userName: user.userName
            }

            chatItems = [...chatItems, chatItemToAdd];

            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        
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
    function toggleSidebar() {
        isUsersSidebarOpen = !isUsersSidebarOpen;
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
    
    async function kickUser(userId: number){
        await handleKickUser(chatId, userId);
        //groupInfo.users = groupInfo.users.filter(user => user.id !== userId);
    }

    async function promoteUser(userId: number){
        await handlePromoteUser(chatId, userId);
    }
    async function demoteUser(userId: number){
        await handleDemoteUser(chatId, userId);
    }
    async function transferOwnership(userId: number){
        await handleTransferOwnership(chatId, userId);
    }
</script>


<svelte:window on:click={closeDropdown} />
<div class="chat-container d-flex flex-column">    
   <div class="chat-header d-flex justify-content-between align-items-center p-3 border-bottom">
        <div>
            <h3>{groupInfo?.name}</h3>
            <i class="fa fa-users" aria-hidden="true" on:click={toggleSidebar}></i>
        </div>
        <div>
            <button class="btn btn-danger" on:click={leaveGroup}>Leave</button>
            {#if isCreator}
            <button class="btn btn-danger" on:click={deleteGroup}>Delete</button>
            {/if}
        </div>
    </div>
    <div class ="d-flex flex-column">
        <div class="chat-body-container d-flex flex-row">
            <div class="chat-body" style="max-width: {isUsersSidebarOpen ? 'calc(100% - 300px)' : '100%'}; overflow-y: auto;">
                {#if isUsersSidebarOpen}
                    <div class="users-sidebar d-flex flex-column" transition:fly={{x: 1000, duration: 500}}>
                        <h5>Group admins: </h5>
                        {#each groupInfo.users as user (user.id)}
                            {#if groupInfo.admins.some(admin => admin.id === user.id)}
                                <div class="user-section">
                                    <div role="navigation" class="user d-flex d-row align-items-center" 
                                        on:focus="{() => isUserHovered = true}" 
                                        on:mouseover="{() => isUserHovered = true}" 
                                        on:blur="{() => isUserHovered = false}" 
                                        on:mouseout="{() => isUserHovered = false}">
                                        <div class="d-flex d-row align-items-center">
                                            <img width="20px" height="20px" src={imageUrl} alt={user.userName} />
                                            <p class="pb-0 mb-0">{user.userName}</p>
                                        </div>
                                        <div class="d-flex flex-row">
                                            <i class="dots fa-solid fa-ellipsis-vertical" on:click|stopPropagation={(event) => openDropdown(user.id, event)}></i>
                                            {#if user.id === userSidebarDropdown}
                                                <div role="navigation" class="dropdown-menu show">
                                                    <a class="dropdown-item" href="#">View Info</a>
                                                    {#if user.id !== userInfo.id && (groupInfo.creatorId === userInfo.id || (groupInfo.admins.some(admin => admin.id === userInfo.id) && user.id !== groupInfo.creatorId))}
                                                        {#if groupInfo.creatorId === userInfo.id}
                                                            <button class="dropdown-item" on:click|stopPropagation={() => transferOwnership(user.id)}>Transfer Ownership</button>
                                                            {#if groupInfo.admins.some(admin => admin.id === user.id)}
                                                                <button class="dropdown-item" on:click|stopPropagation={() => demoteUser(user.id)}>Demote to Member</button>
                                                            {/if}
                                                        {/if}
                                                        <div class="dropdown-divider"></div>
                                                        <button class="dropdown-item" on:click|stopPropagation={() => kickUser(user.id)}>Kick</button>
                                                        <button class="dropdown-item" on:click|stopPropagation={null}>Ban</button>
                                                    {/if}
                                                </div>
                                            {/if}
                                        </div>
                                    </div>
                                </div>
                            {/if}
                        {/each}
                        <h5>Group members: </h5>
                        {#each groupInfo.users as user (user.id)}
                            {#if !groupInfo.admins.some(admin => admin.id === user.id)}
                                <div class="user-section">
                                    <div role="navigation" class="user d-flex d-row align-items-center" 
                                        on:focus="{() => isUserHovered = true}" 
                                        on:mouseover="{() => isUserHovered = true}" 
                                        on:blur="{() => isUserHovered = false}" 
                                        on:mouseout="{() => isUserHovered = false}">
                                        <div class="d-flex d-row align-items-center">
                                            <img width="20px" height="20px" src={imageUrl} alt={user.userName} />
                                            <p class="pb-0 mb-0">{user.userName}</p>
                                        </div>
                                        <div class="d-flex flex-row">
                                            <i class="dots fa-solid fa-ellipsis-vertical" on:click|stopPropagation={(event) => openDropdown(user.id, event)}></i>
                                            {#if user.id === userSidebarDropdown}
                                                <div role="navigation" class="dropdown-menu show">
                                                    <a class="dropdown-item" href="#">View Info</a>
                                                    <div class="dropdown-divider"></div>
                                                    {#if user.id !== userInfo.id && (groupInfo.creatorId === userInfo.id || (groupInfo.admins.some(admin => admin.id === userInfo.id) && user.id !== groupInfo.creatorId))}
                                                        {#if groupInfo.creatorId === userInfo.id}
                                                            <button class="dropdown-item" on:click|stopPropagation={() => transferOwnership(user.id)}>Transfer Ownership</button>
                                                            {#if groupInfo.admins.some(admin => admin.id === user.id)}
                                                                <button class="dropdown-item" on:click|stopPropagation={null}>Demote to Member</button>
                                                            {/if}
                                                        {/if}
                                                        {#if groupInfo.creatorId === userInfo.id || groupInfo.admins.some(admin => admin.id === userInfo.id)}
                                                            <button class="dropdown-item" on:click|stopPropagation={() => promoteUser(user.id)}>Promote to Admin</button>
                                                        {/if}
                                                        <div class="dropdown-divider"></div>
                                                        <button class="dropdown-item" on:click|stopPropagation={() => kickUser(user.id)}>Kick</button>
                                                        <button class="dropdown-item" on:click|stopPropagation={null}>Ban</button>
                                                    {/if}
                                                </div>
                                            {/if}
                                        </div>
                                    </div>
                                </div>
                            {/if}
                        {/each}
                    </div>
                {/if}
                <div style="max-height:49rem; overflow-y: auto;" bind:this={scrollContainer}>
                    <div style="position: relative;overflow: hidden;max-width: 100%;padding: 8px 0.5em 0 1.5em;transition: height 200ms,background-color 200ms;word-wrap: break-word;">
                        <div style="position: relative;display: table;width: 100%;padding: 0 0 0 5px;margin: 0 auto;table-layout: fixed;">
                            
                            {#each chatItems as item}
                            <div class="message" style="display: flex; align-items: start;">
                                
                                <div id="img">
                                    <button class="message-avatar-wrap">
                                        <span class="message-avatar">
                                            <img style="width:32px; height:32px;" alt="User" src={imageUrl}>
                                        </span>
                                    </button>
                                </div>
                                <div>
                                    {#if item.content === 'UserJoined' || item.content === 'UserLeft' || item.content === 'UserPromoted' || item.content === 'UserDemoted' || item.content === 'OwnershipTransferred'}
                                        <p style="color: {item.content === 'UserPromoted' ? 'green' : item.content === 'UserDemoted' ? 'red' : item.content === 'OwnershipTransferred' ? 'yellow' : 'grey'}; font-style: italic;">
                                            {item.userName} has {item.content === 'UserJoined' ? 'joined' : item.content === 'UserLeft' ? 'left' : item.content === 'UserPromoted' ? 'been promoted in' : item.content === 'UserDemoted' ? 'been demoted in' : 'transferred ownership in'} the chat
                                        </p>
                                    {:else if item.content === 'UserKicked'}
                                        <p style="color: red; font-style: italic;">
                                            {item.userName} has been kicked from the chat
                                        </p>
                                    {:else}
                                        <div class="message-header">
                                            <div class="message-sender">
                                                <button class="message-sender-button">{item.userName}</button>
                                            </div>
                                            <div class="message-date">
                                                <a href="/" style="display: inline-block;color: inherit;">{item.timestamp}</a>
                                            </div>              
                                        </div>   
                                        <div class="message-body">                    
                                            <p>{item.content}</p>                                           
                                        </div>
                                        {#each chatItems as item (item.id)}
                                            {#if userInfo && item.userName === userInfo.userName && !(item.content in ['UserJoined', 'UserLeft', 'UserPromoted', 'UserDemoted', 'UserKicked', 'OwnershipTransferred'])}
                                                <div class="message-actions">
                                                    <i class="icon-edit">✎</i>
                                                    <i class="icon-delete">✖</i>
                                                </div>
                                            {/if}
                                        {/each}
                                    {/if}
                                </div>
                            </div>
                        {/each}

                        </div>
                    </div>
                </div>   
            </div>
        </div>   
    </div>

    

    <div class="chat-footer" style="max-width: {isUsersSidebarOpen ? 'calc(100% - 300px)' : '100%'};" >
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



<style>
    .chat-container{
        flex-grow: 1;
        display: flex;
        flex-direction: column;
        height: 100%;
        overflow: hidden;
        
    }
    .chat-body-container{
    flex-grow: 1;
    display: flex;
    overflow: auto; /* add scrolling if the content overflows */
}

.message {
    transition: background-color 100ms;
    position:relative;
}
.message-actions {
    position: absolute;
    top: 0;
    right: 0;
    display: flex;
    justify-content: flex-end;
    padding: 4px;
    visibility: hidden;
}
.message:hover {
    background-color: #f5f5f5;
}

.message:hover .message-actions {
    visibility: visible;
}

.icon-edit, .icon-delete {
    margin-left: 8px;
    cursor: pointer;
}

.chat-header{
    position: relative;
    z-index: 15;
    width: 100%;
    max-height: 63px;
    flex: 0 0 63px;
    border-bottom: 1px solid rgba(var(--center-channel-color-rgb), 0.12);
    background: var(--center-channel-bg);
    font-size: 14px;
}
    .chat-header h3{
        margin-bottom:2px;
    }
    .chat-header i{
        cursor:pointer;
        
        padding-top:4px;
        margin-bottom:8px;
        padding-bottom:4px;

        padding-left:2px;
        padding-right:2px;
        border-radius: 5px;
    }
        .chat-header i:hover{
            background-color: #b4b4b4;
        }
    
.message-header{
    position:relative;
    display: flex;
    width: 100%;
    margin-bottom: 2px;
    white-space: nowrap;
}
    .message-avatar-wrap{
        position: relative;
        display: inline-block;
        height: 32px;
        padding: 0;
        border: none;
        background: transparent;
    }
        .message-avatar{
            display: inline-flex;
            overflow: hidden;
            align-items: center;
            justify-content: center;
        }
        #img {
        min-width: 40px; /* adjust as needed */
        flex-shrink: 0;
        }
    .message-sender{
        display: flex;
        min-width: 0;
        flex: 0 auto;
        margin-right: 8px;
        font-weight: 600;
        overflow: hidden;
        text-overflow: ellipsis;
    }
        .message-sender-button{
            border: none;
            background: transparent;
        }
    .message-date{
        max-width: 100%;flex-basis: 0;flex-grow: 1;
    }

        

.message-body{
    transition-property: height ;transition-duration: 250ms;transition-timing-function: ease;height: auto;overflow: visible;
    width: 100%; word-wrap:break-word;
    position: relative;overflow: clip;
    max-height:600px;
    text-align: left;
    margin-left:5px;


}

.users-sidebar {
        position: fixed;
        right: 0;
        width: 300px; /* adjust as needed */
        height: 100%;
        background-color: #f1f1f1; /* adjust as needed */
        overflow-y: auto;
        padding: 20px;
        box-sizing: border-box;
        transition: transform 0.3s ease-in-out;
        transform: translateX(0);
        flex-shrink: 0;
        border-left: solid 1px #e5e5e5;
}

    .user-section{
        margin-bottom: 10px;
    }
        .user{
            position:relative;
            display:flex;
            justify-content: space-between;
            align-items: center;
        }
            .dropdown-menu{
                position: absolute;
                top: 100%; /* This positions the dropdown right under the user div */
                right: 0;
            }
            .dots{
                visibility: hidden;
                margin-left: 15px;
                margin-right: 15px;
                padding-left:10px;
                padding-right:10px;
                padding-top:2px;
                padding-bottom:2px;
                border-radius: 10px;
                
            }
                
                
        .user:hover .dots{
            visibility: visible;
            
            
        }
            .user:hover .dots:hover{
                cursor: pointer;
                background-color: #a8a8a8;
            }
        .user:hover{
            background-color: #e5e5e5;
        }
        .user img{
            margin-right:5px
        }


    
.chat-footer{
    overflow-y: auto; 
    margin-left: 5px;
    margin-right:5px;
}



 
</style>