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
    import "$lib/css/chatstyles.css";



    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let imageUrl = '/user-icon-placeholder.png'

    let userInfo: User;
    let scrollContainer: any;
    let groupInfo: Group;
    let chatItems: ChatItem[] = [];
    let chatUsers: User[]= [];
    let isUsersSidebarOpen = false;
    let isPickerOpen = false;
    let EmojiPicker: any;
    let userSidebarDropdown : any = null;
    

    let sendMessageText: string = "";
    let chatId: string;
    let isCreator: boolean;

    let isLoading = false;
    let pageCurrent = 1;
    const pageSize = 40;
    
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
        if (typeof window !== 'undefined') {
            const module = await import('emoji-picker-element');
            EmojiPicker = module.default;
        }
        ready = true;
    });

    
    

    async function loadMoreMessages() {
        if (isLoading) return;
        isLoading = true;
        const newMessages = await fetchMessages(chatId, pageCurrent, pageSize);
        chatItems = [...newMessages, ...chatItems];
        pageCurrent++;
        isLoading = false;
    }

    async function loadMessages() {
        chatItems = await fetchMessages(chatId, pageCurrent, pageSize);
        pageCurrent++;
    }

    function handleScroll(event: any) {
        const { scrollTop, clientHeight, scrollHeight } = event.target;
        const atTop = scrollTop === 0;
        if (atTop && !isLoading) {
            loadMoreMessages();
        }
    }


    function createChatItem(data: any, eventType: string): ChatItem {
    const dateObject = new Date();
    const time = new Intl.DateTimeFormat('default', { hour: '2-digit', minute: '2-digit', hour12: false }).format(dateObject);

    let chatItem: Partial<ChatItem> = {
        id: data.message.id,
        timestamp: Date.now(),
        date: dateObject.toLocaleDateString(),
        time: time,
        isEvent: eventType !== 'ReceiveMessage',
        withoutDetails: false,
        userPfp: `${backendUrl}${data.userPfp}`,
    };

    if (eventType === 'ReceiveMessage') {
        if (chatItems.length > 0) {
            const lastMessage = chatItems[chatItems.length - 1];
            console.log('Last message: ', lastMessage.timestamp)
            const lastMessageTime = new Date(lastMessage.timestamp);
            const newMessageTime = new Date();
            const timeDifference = (newMessageTime.getTime() - lastMessageTime.getTime()) / 60000; // difference in minutes
            console.log('New message: ', newMessageTime);

            chatItem = {
                ...chatItem,
                content: data.message.text,
                userName: data.message.fromUser.userName,
                userId: data.message.fromUserId,
                isActive: data.message.fromUser.isActive,
                withoutDetails: lastMessage.userId.toString() === data.message.fromUser.id.toString() && timeDifference < 5
            };
        } else {
            chatItem = {
                ...chatItem,
                content: data.message.text,
                userName: data.message.fromUser.userName,
                userId: data.message.fromUserId,
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
    
                let chatItemToAdd = createChatItem(data, 'ReceiveMessage');
                chatItems = [...chatItems, chatItemToAdd];
                console.log("chatItem: ", chatItemToAdd.withoutDetails);

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
    <ChatHeader  bind:groupInfo bind:isUsersSidebarOpen {userInfo} {chatId} />       
    {#if isUsersSidebarOpen}
        <UserSidebar bind:isUsersSidebarOpen {groupInfo} {userSidebarDropdown} {userInfo} {imageUrl} {chatId}/>
    {/if}           
    <ChatMessage {groupInfo} {chatId} {chatItems} {userInfo} {imageUrl} bind:scrollContainer {handleScroll}/>  
    <div class="chat-footer" style="min-height:6rem; height:6rem;" >
        <div class="send-message-wrap">
            <div class="textarea-container">
                <textarea placeholder="Type a message..." bind:value={sendMessageText} 
                    on:keydown={e => {
                        if (e.key === 'Enter') {
                            e.preventDefault();
                            sendMessage();
                        }
                    }}></textarea>
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
    
</div>
{/if}

