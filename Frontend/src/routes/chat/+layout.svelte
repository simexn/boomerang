<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { page } from '$app/stores'
    import { onMount } from 'svelte';
    import { fade, slide } from 'svelte/transition';
    import {handleJoinRoom, handleRoomSubmit} from '$lib/Handlers/groupHandler'
    import {fetchChats} from '$lib/Handlers/groupHandler'

    let createModalActive = false;
    let joinModalActive = false;
    let newGroupName: string;
    let inviteCode: string;

    export let chats: any =[];
    
    onMount(async() => {
       chats = await fetchChats();
    });

    async function createNewChat(event: Event){
        event.preventDefault();
        const formData = {
            newGroupName,
            inviteCode
        }
        chats = await handleRoomSubmit(formData);
        newGroupName = "";
        createModalActive = false;
    }  

    async function joinGroup(event: Event){
        event.preventDefault();

        
        chats = await handleJoinRoom(inviteCode);
        inviteCode = "";
        joinModalActive = false;
    }
</script>
<main>
    <div class="side-menu">
        <div>
            {#each chats as chat (chat.id)}
                <a class="room-button" href="../chat/{chat.id}">{chat.name}</a>
            {/each}
            <button class="room-button" id="create-room-button" on:click={() => createModalActive = true}>+</button>
        </div>
        <button class="room-button" id="join-room-button" on:click={() => joinModalActive = true}>Join a Chat</button> <!-- New button -->
    </div>
    <slot/>    
</main>
{#if (createModalActive)}
<div class="modal" id="create-room-modal">      
    <form class="modal-body" on:submit={createNewChat} in:slide out:fade={{duration:100}} action="">
        <button class="close" on:click={() => createModalActive = false}>X</button>
        <header>Create room</header>
        <div>
            <input name="name" bind:value={newGroupName}>
            <input name="inviteCode" bind:value={inviteCode}>	
        </div>
        <footer>
            <button type="submit">Create</button>
        </footer>
    </form>
</div>
{/if}

{#if (joinModalActive)}
<div class="modal" id="create-room-modal">      
    <form class="modal-body" on:submit={joinGroup} in:slide out:fade={{duration:100}} action="">
        <button class="close" on:click={() => joinModalActive = false}>X</button>
        <header>Join a room</header>
        <div>
            <input name="inviteCode" bind:value={inviteCode}>
            	
        </div>
        <footer>
            <button type="submit">Create</button>
        </footer>
    </form>
</div>
{/if}

<style>    
    main{
       flex-grow:1;
       display: flex;
       flex-direction: row;
   }

  
   
   .side-menu{
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    background-color:var(--sec);
    min-width:100px;
    z-index: 50;
    height: 100%; /* Make sure the side-menu takes up the full height */
}
   .room-button{
       color: white;
       background-color: darkgray;
       text-decoration: none;
       display: flex;
       justify-content: center;
       align-items: center;
       margin: 0.75rem 1rem;
       height: 50px;
       border-radius: 5px;
       box-shadow: 0 1px 3px 1px black;
       min-width: 6rem;
   }
   .room-button:hover{
       box-shadow: 0px 2px 3px 1px black;
   }

   .modal{
       display: flex;
       position: absolute;
       justify-content: center;
       align-items: center;
       flex-direction: column;
       min-height: 100vh;
       min-width: 100vw;
       z-index: 200;
       top:0;
       left:0;
       background-color: rgba(0.2, 0.2, 0.2, 0.8);
   }
 
   .modal-body > .close{
       position:absolute;
       top:5px;
       right: 5px;
       margin-top: 0;
   }
   .modal-body{
       position:relative;
       min-width:300px;
       display:flex;
       flex-direction: column;
       background-color: yellow;
       padding:2rem;
   }

   .modal-body > *{
       margin-top: 0.5rem;
   }
   .modal-body > header{
       color: #FFF;
       font-size:24px;
   }
   .modal-body>div>input{
       width:100%;
   }
   .modal-body > footer{
       text-align: center;
   }
</style>