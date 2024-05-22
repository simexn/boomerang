<script lang="ts">
    import { page } from '$app/stores';
    import { tick } from 'svelte';
    import { handleMessageSubmit, fetchMessages } from "$lib/handlers/chatHandler";
    import {getToken} from "$lib/handlers/authHandler"; 
    import { onMount } from 'svelte';
    import { HubConnectionBuilder } from '@microsoft/signalr';
    import { fetchGroupInfo} from '$lib/handlers/groupHandler';
    import type { User } from "$lib/handlers/accountHandler";
    import type { Group } from "$lib/handlers/groupHandler";
    import type {ChatItem} from "$lib/handlers/chatHandler";
    import { userStore} from '$lib/stores/userInfoStore';
    import { goto } from '$app/navigation';
    import UserSidebar from '$lib/components/chat/UserSidebar.svelte';
    import ChatHeader from '$lib/components/chat/ChatHeader.svelte';
    import ChatMessage from '$lib/components/chat/ChatMessage.svelte';
    import "$lib/css/chatstyles.css";
    import GroupInfoSidebar from '$lib/components/chat/GroupInfoSidebar.svelte';
    const backendUrl = import.meta.env.VITE_BACKEND_URL;
    let imageUrl = '/user-icon-placeholder.png'
    let userInfo: User;
    let scrollContainer: any;
    let groupInfo: Group;
    let chatItems: ChatItem[] = [];
    let isUsersSidebarOpen = false;
    let isInfoSidebarOpen = false;
    let isPickerOpen = false;
    let EmojiPicker: any;
    let userSidebarDropdown : any = null;
    let sendMessageText: string = "";
    let fileInput: any;
    let file: any;
    let fileName = '';
    let maxFileSize = 2 * 1024 * 1024;
    function handleFileUpload() {
        if (fileInput.files.length > 0) {
            file = fileInput.files[0];
            fileName = file.name;
            if (file.size > maxFileSize) {
                alert("File size exceeds the limit of 2MB");
                file=null;
                fileName = '';
                return;
            }
        }
    }
    function removeFile() {
        file = null;
        fileName = '';
        fileInput.value = '';
    }
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
        pageCurrent = 1;
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
        const lastMessageTime = new Date(lastMessage.timestamp);
        const newMessageTime = new Date();
        const timeDifference = (newMessageTime.getTime() - lastMessageTime.getTime()) / 60000; // difference in minutes
        chatItem = {
            ...chatItem,
            content: data.message.text,
            userName: data.message.fromUser.userName,
            userId: data.message.fromUser.id,
            isActive: data.message.fromUser.isActive,
            withoutDetails: lastMessage.userId === data.message.fromUser.id && timeDifference < 5,
            fileUrl: data.message.fileUrl
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
    
                let chatItemToAdd = createChatItem(data, 'ReceiveMessage');
                chatItems = [...chatItems, chatItemToAdd];
                console.log("chatItem: ", chatItemToAdd.withoutDetails);

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });

            connection.on("UserJoined", async function(data: any) {
                console.log("User joined: ", data, );
                groupInfo.users = [...groupInfo.users, data.user];

                groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== data.user.id);

                let chatItemToAdd = createChatItem(data, 'UserJoined');
                console.log("chatItem: ", chatItemToAdd);

                chatItems = [...chatItems, chatItemToAdd];

                await tick();
                scrollContainer.scrollTop = scrollContainer.scrollHeight;
            });
        connection.on("UserLeft", async function(data: any) {
            groupInfo.users = groupInfo.users.filter(u => u.id !== data.user.id);
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== data.user.id);
            let chatItemToAdd = createChatItem(data, 'UserLeft');
            chatItems = [...chatItems, chatItemToAdd];
            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserKicked", async function(data: any) {
            if (data.user.id === userInfo.id) {
                goto('/chat/home');
                
            }
            groupInfo.users = groupInfo.users.filter(u => u.id !== data.user.id);
            let chatItemToAdd = createChatItem(data, 'UserKicked');
            chatItems = [...chatItems, chatItemToAdd];
            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });
        
        connection.on("UserBanned", async function(data: any){
            if (data.user.id === userInfo.id) {
                location.reload();
                goto('/chat/home');
            }
            groupInfo.users = groupInfo.users.filter(u => u.id !== data.user.id);
            groupInfo.bannedUsers = groupInfo.bannedUsers ? [...groupInfo.bannedUsers, data.user] : [data.user];
            let chatItemToAdd = createChatItem(data, 'UserBanned');
            chatItems = [...chatItems, chatItemToAdd];
            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserUnbanned", async function(data: any){
            console.log("User unbanned: ", data);
            groupInfo.bannedUsers = groupInfo.bannedUsers.filter(u => u.id.toString() !== data.id.toString());
            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserPromoted", async function(data: any){
            groupInfo.admins = [...groupInfo.admins, data.user];
            let chatItemToAdd = createChatItem(data, 'UserPromoted');
            chatItems = [...chatItems, chatItemToAdd];
            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("UserDemoted", async function(data: any){
            groupInfo.admins = groupInfo.admins.filter(admin => admin.id !== data.user.id);
            let chatItemToAdd = createChatItem(data, 'UserDemoted');
            chatItems = [...chatItems, chatItemToAdd];
            await tick();
            scrollContainer.scrollTop = scrollContainer.scrollHeight;
        });

        connection.on("OwnershipTransferred", async function(data: any){
            console.log("Ownership transferred to: ", data);
            groupInfo = { ...groupInfo, creatorId: data.user.id, admins: [...groupInfo.admins, data.user] };
            let chatItemToAdd = createChatItem(data, 'OwnershipTransferred');
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
    async function getGroupInfo(){       
        groupInfo = await fetchGroupInfo(chatId);        
    }
    async function sendMessage() {
        message = await handleMessageSubmit(sendMessageText, chatId, file);
        sendMessageText = "";
        file = null;
        fileName = '';
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
    <ChatHeader  bind:groupInfo bind:isInfoSidebarOpen bind:isUsersSidebarOpen {userInfo} {chatId} />       
    {#if isUsersSidebarOpen}
        <UserSidebar bind:isUsersSidebarOpen {groupInfo} {userSidebarDropdown} {userInfo} {imageUrl} {chatId}/>
    {/if}           
    <ChatMessage {groupInfo} {chatId} {chatItems} {userInfo} {imageUrl} bind:scrollContainer {handleScroll}/>  
    <div class="chat-footer">
        <div class="send-message-wrap">
            <div class="textarea-container">
                <textarea placeholder="Въведете съобщение..." bind:value={sendMessageText} 
                    on:keydown={e => {
                        if (e.key === 'Enter') {
                            e.preventDefault();
                            sendMessage();
                        }
                    }}></textarea>
            </div>
            {#if fileName}
                <div class="file-preview">
                    <div class="file-name">
                        {fileName}
                        <button on:click={removeFile}>X</button> <!-- New button to remove the file -->
                    </div>
                    {#if file && file.type.startsWith('image/')} <!-- Preview for image files -->
                        <img class="imgfilepreview" src={URL.createObjectURL(file)} alt={fileName} />
                    {/if}
                </div>
            {/if}
            <!-- svelte-ignore a11y-click-events-have-key-events -->
            <!-- svelte-ignore a11y-no-static-element-interactions -->
            <div class="message-options">
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <div class="d-flex flex-row">
                <div class="message-option emoji" on:click={togglePicker}>
                    <i class="fa-regular fa-face-smile"></i>
                    {#if isPickerOpen}
                    <emoji-picker class="light" on:emoji-click={addEmoji}></emoji-picker>
                    {/if}
                </div>
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <div class="message-option emoji">
                    <input type="file" bind:this={fileInput} on:change={handleFileUpload} style="display: none" />
                    <i class="fa fa-upload" on:click={() => fileInput.click()}></i>
                </div>
                </div>
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <div class="send-message-button" class:disabled={sendMessageText == null || sendMessageText == ""} on:click={sendMessage}>
                    
                    <i class="material-icons" class:disabled={sendMessageText == null || sendMessageText == ""}>send</i>
                </div>
            </div>
        </div>
    </div>  
    {#if isInfoSidebarOpen}
        <GroupInfoSidebar bind:isInfoSidebarOpen bind:groupInfo {userInfo} {chatId}/>
    {/if}
</div>
{/if}

