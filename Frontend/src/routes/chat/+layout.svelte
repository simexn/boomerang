<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { page } from '$app/stores'
    import { onMount } from 'svelte';
    import { fade, slide, fly } from 'svelte/transition';
    import {handleJoinRoom, handleRoomSubmit} from '$lib/handlers/groupHandler'
    import {fetchChats} from '$lib/handlers/groupHandler'
    import JoinGroupModal from './JoinGroupModal.svelte'
    import AddFriendModal from './AddFriendModal.svelte';
    import { fetchFriends, type FriendInfo, type FriendPreview } from '$lib/handlers/userHandler';
    import { friendsStore } from '$lib/stores/friendsStore';
    import { userStatuses } from '$lib/stores/userStatusesStore';
    import { sidebarOpen } from '$lib/stores/sidebarToggleStore';

    let imgUrl= '/user-icon-placeholder.png';
    
    let friendsDropdownActive = true;
    let groupDropdownActive = true;
    let friendsModalActive = false;
    let groupModalActive = false;
    
    let activeChatId: any = $page.params.chat;

    $: {
        
        
    }
    
    
    export let groupChats: any =[];

    
    onMount(async() => {
        await fetchFriends();
        groupChats = await fetchChats();
        activeChatId = Number(sessionStorage.getItem('activeChatId'));
    });
    
    function setActiveChat(chatId: any) {
        activeChatId = Number(chatId);
        sessionStorage.setItem('activeChatId', chatId);
    }


    
</script>

<div class="snd-layout-wrapper">
    <div class="sidebar d-flex flex-column flex-shrink-0 p-3 text-white" class:open={$sidebarOpen}>
        <div class="d-flex flex-row">
            <button class="dropdown-button" on:click={() => friendsDropdownActive = !friendsDropdownActive}><span>Приятели
            <i class="fa fa-caret-right" class:rotate={friendsDropdownActive}></i></span></button>
            <div class="add-group-button-wrap">
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <i class="fa fa-plus add-group-button" on:click={() => friendsModalActive = true}></i>
            </div>
        </div>
       <hr class="mb-1" style="padding: 0; margin:0; width:100%">
       {#if friendsDropdownActive}
       <div class="dropdown-container" class:active={friendsDropdownActive} transition:slide={{duration: 500}}> 
            <ul class="nav nav-pills flex-column mb-auto">
                {#each $friendsStore as friend}
                <li class="nav-item" style="" transition:slide={{duration: 300}}>
                    <a class:active={activeChatId === friend?.id} class="nav-link sidebar-group" href={`/chat/me/${friend?.chatId}`} on:click={() => {setActiveChat(friend?.id); $sidebarOpen = false;}}>
                        <!-- svelte-ignore a11y-missing-attribute -->
                        <img width="40px" height="40px" style="border-radius: 50%" src="{friend.userPfp}">
                        <span class="status-dot" class:online={$userStatuses[friend?.id.toString()] == 'online'} class:away={$userStatuses[friend?.id.toString()] == 'away'}></span>
                        <b>{friend?.username}</b>
                    </a>
                </li>
                {/each}
                
            </ul>
        </div>
        <hr class="mt-1" style="padding: 0; margin:0; width:100%" transition:slide={{duration: 50}}>
        {/if}
        
        <div class="d-flex flex-row">
            <button class="dropdown-button" on:click={() => groupDropdownActive = !groupDropdownActive}><span>Групи
            <i class="fa fa-caret-right" class:rotate={groupDropdownActive}></i></span></button>
            <div class="add-group-button-wrap">
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <i class="fa fa-plus add-group-button" on:click={() => groupModalActive = true}></i>
            </div>
        </div>
       <hr class="mb-1" style="padding: 0; margin:0; width:100%">
       {#if groupDropdownActive}
       <div class="dropdown-container" class:active={groupDropdownActive} transition:slide={{duration: 500}}> 
            <ul class="nav nav-pills flex-column mb-auto">
                {#each groupChats as chat, index (chat.id)}
                <li class="nav-item" style="" transition:slide={{duration: 300}}>
                    <a class:active={activeChatId === chat.id} class="nav-link sidebar-group" href={`/chat/${chat.id}`} on:click={() => {setActiveChat(chat.id); $sidebarOpen = false}}><b>{chat.name}</b></a>
                </li>
                {/each}
                
            </ul>
        </div>
        <hr class="mt-1" style="padding: 0; margin:0; width:100%" transition:slide={{duration: 50}}>
        {/if}
    </div>
    <div class="chat-content">
        <slot/>
    </div>
</div>

          
{#if (friendsModalActive)}
    <AddFriendModal bind:friendsModalActive/>
{/if}

{#if (groupModalActive)}
    <JoinGroupModal bind:groupModalActive bind:groupChats/>
{/if}

    <style>
        .snd-layout-wrapper {
            display:flex;
            flex-direction:row;
            overflow: hidden !important;
            height: 100% !important;
            max-height: 100% !important;
            z-index: 500;
        }
        
        .chat-content{
            width: 100vw;
            height: 100%;
        }
        .sidebar{
            background-color: var(--sec);
            padding-top:0 !important;
            width: 15rem;
            transition: width 0.3s ease;
            overflow: hidden;
            
        }

        @media (max-width: 768px) {
            .sidebar.open{
                width:100%;
                
            }
            .sidebar:not(.open){
                width:0;
                padding: 0 !important;
            }
        }
        .sidebar-group{
            color: var(--prim-fg);
            text-align:center;
            display: block;
            width: 100%;
            
        }

        .nav-link{
            padding:1rem !important;
        }
        
        
        .dropdown-button {
            display: flex;
            
            text-decoration: none;
            font-size: 26px;
            color: #818181;
            border: none;
            background: none;
            width: calc(80% + 2*1rem);
            text-align: left;
            cursor: pointer;
            outline: none;
            margin-left: -1rem;
            padding-right:0;
            padding-top: 1rem;
            padding-bottom: 1rem;
            border-bottom: #818181;
        }
        .dropdown-button:hover {
            color: #f1f1f1;
            background-color: var(--sec-tp);
        }

        .fa-caret-right{
            transition: transform 0.3s cubic-bezier(0.68, -0.55, 0.27, 1.55);
        }
        
        .rotate{
            transform: rotate(90deg);
                
        }
    .add-group-button{
        border: none;
        background:transparent;
        color: #818181;
        padding: 5px;
       
    }

    .add-group-button:hover{
        border-radius: 5px;
        color: #f1f1f1;
        background-color: var(--sec-tp);
    }


    .add-group-button-wrap{
        width: 28px;
        height: 71px;
        margin: 2px 16px 2px 16px;

        display: flex;
        align-items: center;
        justify-content: center;
    }
    
    .status-dot {
    height: 10px;
    width: 10px;
    background-color: #bbb;
    border-radius: 50%;
    display: inline-block;
    }
    .status-dot.online {
    background-color: #4CAF50;
    }
    .status-dot.away{
        background-color: #FFC107;
    }
    
        
    </style>