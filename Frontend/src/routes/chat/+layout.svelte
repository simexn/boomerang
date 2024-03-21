<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { page } from '$app/stores'
    import { onMount } from 'svelte';
    import { fade, slide, fly } from 'svelte/transition';
    import {handleJoinRoom, handleRoomSubmit} from '$lib/Handlers/groupHandler'
    import {fetchChats} from '$lib/Handlers/groupHandler'

    
    let activeTab = 'create';
    let isNewGroupNameValid = true;
    let isInviteCodeValid = true;
    let modalBody: any;
    let groupDropdownActive = true;
    let modalActive = false;
    let newGroupName: string;
    let inviteCode: string;
    let activeChatId: any = null;

    export let chats: any =[];
    
    onMount(async() => {
        chats = await fetchChats();
        activeChatId = Number(sessionStorage.getItem('activeChatId'));
    });
    
    function setActiveChat(chatId: any) {
        activeChatId = Number(chatId);
        sessionStorage.setItem('activeChatId', chatId);
    }

    async function createNewChat(event: Event){
        event.preventDefault();
        const formData = {
            newGroupName,
            inviteCode
        }
        if(formData.newGroupName == null || formData.newGroupName == undefined || formData.newGroupName.length < 1 ){
            isNewGroupNameValid = false;
            return;
        }

        if(formData.inviteCode.length < 8 && formData.inviteCode.length > 1){
            isInviteCodeValid = false;
            return;
        }
        isInviteCodeValid = true;
        chats = await handleRoomSubmit(formData);
        newGroupName = "";
        inviteCode = "";
        modalActive = false;
    }  

    async function joinGroup(event: Event){
        event.preventDefault();

        if(inviteCode.length < 8 && inviteCode.length > 1){
            isInviteCodeValid = false;
            return;
        }
        isInviteCodeValid = true;
        chats = await handleJoinRoom(inviteCode);
        inviteCode = "";
        modalActive = false;
    }
</script>

<div style=" display:flex; flex-direction:row;">
    <div class="sidebar d-flex flex-column flex-shrink-0 p-3 text-white">
        <div class="d-flex flex-row">
            <button class="dropdown-button" on:click={() => groupDropdownActive = !groupDropdownActive}><span>Groups
            <i class="fa fa-caret-right" class:rotate={groupDropdownActive}></i></span></button>
            <div class="add-group-button-wrap">
                <i class="fa fa-plus add-group-button" on:click={() => modalActive = true}></i>
            </div>
        </div>
       <hr class="mb-1" style="padding: 0; margin:0; width:100%">
       {#if groupDropdownActive}
       <div class="dropdown-container" class:active={groupDropdownActive} transition:slide={{duration: 500}}> 
            <ul class="nav nav-pills flex-column mb-auto">
                {#each chats as chat, index (chat.id)}
                <li class="nav-item" style="" transition:slide={{duration: 300}}>
                    <a class:active={activeChatId === chat.id} class="nav-link sidebar-group" href="../chat/{chat.id}" on:click={() => setActiveChat(chat.id)}><b>{chat.name}</b></a>
                </li>
                {/each}
                
            </ul>
        </div>
        {/if}
        <hr class="mt-1" style="padding: 0; margin:0; width:100%">
    </div>
    <div class="chat-content">
        <slot/>
    </div>
</div>

          

    

{#if (modalActive)}
<div class="modal" transition:fade={{duration: 200}}>
    <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content">
            <div class="modal-header p-0">
                <div class="d-flex flex-row justify-content-between w-100">
                    <h5 class="modal-title btn w-50 h-50 pt-1 pb-1 {activeTab === 'create' ? 'active' : ''}" on:click={() => activeTab = 'create'}>Create a chat</h5>
                    <h5 class="modal-title btn w-50 h-50 pt-1 pb-1 {activeTab === 'join' ? 'active' : ''}" on:click={() => activeTab = 'join'}>Join a chat</h5>
                </div>
            </div>
            <div class="modal-body" bind:this={modalBody}>
                {#if activeTab === 'create'}
                    <!-- Content for 'Create a chat' -->
                    <form on:submit|preventDefault={createNewChat} in:fly={{x: -200, duration: 500}} on:introstart={() => modalBody.style.overflowY = 'hidden'} on:outroend={() => modalBody.style.overflowY = 'auto'}>
                        <div class="mb-3">
                            <label for="newGroupName" class="form-label">Group Name</label>
                            <input type="text" bind:value={newGroupName} class="form-control" id="newGroupName" class:invalid-input={!isNewGroupNameValid} required>
                            {#if !isNewGroupNameValid}
                                <div class="invalid-feedback">
                                    Group name must be at least 1 character long.
                                </div>
                            {/if}
                        </div>
                        <div class="mb-3">
                            <label for="inviteCodeCreate" class="form-label">Invite Code</label>
                            <input type="text" maxlength="8" placeholder="(optional)" bind:value={inviteCode} class="form-control" id="inviteCodeCreate" class:invalid-input={!isInviteCodeValid}>
                            {#if !isInviteCodeValid}
                                <div class="invalid-feedback">
                                    Invite code must be 8 characters long.
                                </div>
                            {/if}
                        </div>
                    </form>
                {:else if activeTab === 'join'}
                    <!-- Content for 'Join a chat' -->
                    <form on:submit|preventDefault={joinGroup} in:fly={{x: 200, duration: 500}} on:introstart={() => modalBody.style.overflowY = 'hidden'} on:outroend={() => modalBody.style.overflowY = 'auto'}>
                        <div class="mb-3">
                            <label for="inviteCodeJoin" class="form-label">Invite Code</label>
                            <input type="text" maxlength="8" bind:value={inviteCode} class="form-control" id="inviteCodeJoin" class:invalid-input={!isInviteCodeValid}>
                            {#if !isInviteCodeValid}
                                <div class="invalid-feedback">
                                    Invite code must be 8 characters long.
                                </div>
                            {/if}
                        </div>
                    </form>
                {/if}
            </div>
            <div class="modal-footer">
                {#if activeTab === 'create'}
                    <button type="submit" class="btn btn-primary" on:click|preventDefault={createNewChat}>Submit</button>
                {:else if activeTab === 'join'}
                    <button type="submit" class="btn btn-primary" on:click|preventDefault={joinGroup}>Submit</button>
                {/if}
                <button type="button" class="btn btn-secondary" on:click={() => modalActive = false}>Close</button>
            </div>
        </div>
    </div>
</div>
{/if}

    <style>
        .modal {                                              
            background-color: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }
            .modal-content{
                width: 20rem;
            }

            h5.modal-title{
                border-radius: 0!important;
            }
            .modal-body {
                height: 300px;
                overflow:hidden;
                overflow-x:hidden; /* Adjust this value as needed */
                overflow-y: auto; /* Enable scrolling if the content is taller than the modal body */
            }
            .invalid-input{
                border: 1px solid red;
            }
            .invalid-feedback{
                display: block;
            }
            .btn.active {
                    background-color: blue; /* Change this to your preferred color */
                    color: white; /* Change this to your preferred color */
                }
        .chat-content{
            width: 100vw;
            height: 100%;
        }
        .sidebar{
            background-color: var(--sec);
            padding-top:0 !important;
            width: 15rem;
            
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
    
    
        
    </style>