<script lang="ts">
    import type {ChatItem} from "$lib/handlers/chatHandler";
    import type { User } from "$lib/handlers/accountHandler";
    import { writable } from "svelte/store";
    import { isLoggedIn } from '$lib/handlers/accountHandler';
    import { userStatuses } from '$lib/stores/userStatusesStore';
    import { onMount } from "svelte";
    import { getToken } from "$lib/handlers/authHandler";
    import { HubConnectionBuilder } from "@microsoft/signalr";
    import { connectionStore } from '$lib/stores/connectionsStore';
    import type { Group } from "$lib/handlers/groupHandler";

    let connection;
    connectionStore.subscribe(value => { connection = value; });
    export let item: ChatItem;
    export let imageUrl: string;
    export let userInfo: User;
    export let isEditingMessage = (id: number, content: string) => {};
    export let deleteMessage = (id: number) => {};
    export let confirmEdit = (id: number, newContent: string) => {};
    export let cancelEdit = (id: number) => {};
    export let groupInfo: Group;
    export let isEditing = writable();
    export let withoutUserDetails: boolean = false;

    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    onMount(async () => {
        // if(await isLoggedIn()){
            
        //     let token = await getToken();
        //     const response = await fetch(`${backendUrl}/account/getUserId`, {
        //         method: 'GET',
        //         headers: {
        //             'Authorization': `Bearer ${token}`,
        //             'Content-Type': 'application/json'
        //         }
        //     });
        //     const data = await response.json();
        
        //     if(response.ok){
        //         let connection = new HubConnectionBuilder()
        //             .withUrl(`${backendUrl}/accountHub`)
        //             .build();

        //             connection.on("UpdateUserStatus", async (userId, status) => {
        //                 console.log("userid" + userId + "status" + status)
                        
        //                 userStatuses.update(statuses => ({ ...statuses, [userId]: status }));
        //             });

        //             await connection.start();

        //             let userId = data.userId.toString();

        //             await connection.invoke("UpdateUserStatus", userId, "online");
        //             }
        //     }
    });
</script>

{#if !item.withoutDetails}
<div style="display:flex; margin-top: 0.5rem;">

<div class="img">
    <div class="message-avatar-wrap">
        <span class="message-avatar">
            <img width="32px" height="32px" style="border-radius: 50%;" alt="User" src={item.userPfp}>
        </span>
        <span class="status-dot" class:online={$userStatuses[item.userId.toString()] == 'online'} class:away={$userStatuses[item.userId.toString()] == 'away'}></span>
    </div>
</div>

    <div>
        <div class="message-header">
            <div class="message-sender">
                <button class="message-sender-button">{item.userName}</button>                            
            </div>
            <div class="message-date tooltips">
                <a href="/" class="message-date-button" style="display: inline-block;color: inherit;" title={item.date}>{item.time}</a>

                    <span class="tooltiptext">{item.date}</span>
             
            </div> 
        </div>   
        <div class="message-body">                    
            {#if $isEditing === item.id}
                <p class="message-edit-info" style=""><i>currently editing message:</i></p>
                <input class="message-editing" bind:value={item.content}/><br>
                <a class="message-edit-confirm" on:click={() => confirmEdit(item.id, item.content)} href="#"><b>save</b></a>
                <a class="message-edit-confirm" on:click={() => cancelEdit(item.id)} href="#"><b>cancel</b></a>
            {:else if item.isDeleted == true}
                <p style="color: grey;"><i>This message has been deleted.</i></p>
            {:else if item.isEdited == true}
                <p style="display: inline-block;">{item.content}</p>
                <p style="display: inline-block; font-size: 12px; color: #B8B8B8;"><i>(edited)</i></p>
            {:else}
                <p>{item.content}</p>
            {/if}                                       
        </div>
        
        {#if userInfo && item.userName === userInfo.userName && $isEditing !== item.id && !item.isDeleted}
            <div class="message-actions">
                <i class="icon-edit fa fa-pencil" on:click={() => isEditingMessage(item.id, item.content)}></i>
                <i class="icon-delete fa fa-trash" on:click={() => deleteMessage(item.id)}></i>
            </div>
        {/if}
    </div>
</div>
{/if}

{#if item.withoutDetails}
<div class="message-container">

<div class="img"></div>
    <div>
        <div class="message-body">                    
            {#if $isEditing === item.id}
                <p class="message-edit-info" style=""><i>currently editing message:</i></p>
                <input class="message-editing" bind:value={item.content}/><br>
                <a class="message-edit-confirm" on:click={() => confirmEdit(item.id, item.content)} href="#"><b>save</b></a>
                <a class="message-edit-confirm" on:click={() => cancelEdit(item.id)} href="#"><b>cancel</b></a>
            {:else if item.isDeleted == true}
                <p style="color: grey;"><i>This message has been deleted.</i></p>
            {:else if item.isEdited == true}
                <p style="display: inline-block;">{item.content}</p>
                <p style="display: inline-block; font-size: 12px; color: #B8B8B8;"><i>(edited)</i></p>
            {:else}
                <p>{item.content}</p>
            {/if}                                       
        </div>
        
        {#if userInfo && item.userName === userInfo.userName && $isEditing !== item.id && !item.isDeleted}
            <div class="message-actions">
                <i class="icon-edit fa fa-pencil" on:click={() => isEditingMessage(item.id, item.content)}></i>
                <i class="icon-delete fa fa-trash" on:click={() => deleteMessage(item.id)}></i>
            </div>
        {/if}
    </div>
</div>
{/if}

<style>
    .img{
        width: 53px;
        padding-right: 10px;
        text-align: right;
        position: relative;
    }
    .message-container {
        display: flex;
    }
.message-time {
    position: absolute;
    top: 0;
    right: 0;
    padding: 5px;
}

    .status-dot {
    height: 10px;
    width: 10px;
    background-color: #bbb;
    border-radius: 50%;
    position:absolute;
    bottom:0;
    right: 0rem;
    }
    .status-dot.online {
    background-color: #4CAF50;
    }
    .status-dot.away{
    background-color: #FFC107;
    }
    .message-actions {
    position: absolute;
    top: -5px;
    right: 0;
    margin-right: .5em;
    display: flex;
    justify-content: flex-end;
    padding: 4px;
    visibility: hidden;
    border:solid 1px;
    border-radius: 15px;
    border-color: rgba(63, 67, 80, 0.2);
    background-color: white;
    
    justify-content: space-evenly;
}



.icon-edit {
    padding: 6px 8px 6px 8px;
    margin-left: 8px;
    margin-right:4px;
    cursor: pointer;
    border-radius: 8px;
    font-size: 1.2em;
}
    .icon-edit:hover{
        background-color: #dddddd;
        color:var(--prim-tp);
    }
.icon-delete{
    padding: 6px 8px 6px 8px;
    margin-right:4px;
    margin-left:8px;
    cursor: pointer;
    border-radius: 8px;
    font-size: 1.2em;
    
}
    .icon-delete:hover{
        background-color: #dddddd;
        color: red;
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
            font-size: 18px;
            font-weight: bold;
        }
    .message-date{
        max-width: 100%;flex-basis: 0;flex-grow: 1;
    }
        .message-date-button{
            display: inline-block;
            font-size: 1em;
            opacity: .6;
            margin-top: .18em;
            text-decoration: none;
        }

        

.message-body{
    transition-property: height ;transition-duration: 250ms;transition-timing-function: ease;height: auto;overflow: visible;
    width: 100%; word-wrap:break-word;
    position: relative;overflow: clip;
    max-height:600px;
    text-align: left;
    margin-left:5px;


}
.message-body p{
    margin:0;
}
.message-editing{
    border-style: solid;
    border-color: #eeeeee;
    border-radius: 10px;

    background-color: var(--sec-fg);
}
.message-edit-info{
    margin:0;
    font-size:12px; 
    color: #acacac;
}

.message-edit-confirm{
    text-decoration:none;
     font-size: 15px;
    margin-left:2.5px;
    margin-right:2.5px;
    cursor:pointer;
}
.tooltips {
    position: relative;
    display: inline-block;
    cursor: pointer;
  }

  .tooltips .tooltiptext {
    visibility: hidden;
    width: 6rem;
    background-color: #555;
    color: #fff;
    text-align: center;
    padding: 0.3rem 0;
    border-radius: 6px;

    position: absolute;
    z-index: 1;
    bottom: 125%; /* Position the tooltip above the text */
    left: 73%;
    margin-left: -3.5rem; /* Use half of the width to center the tooltip */

    opacity: 0;
    transition: opacity 0.3s;
  }

  .tooltips:hover .tooltiptext {
    visibility: visible;
    opacity: 1;
  }
  .tooltips .tooltiptext::after {
    content: "";
    position: absolute;
    top: 100%; /* Position the arrow at the bottom of the tooltip */
    left: 50%;
    margin-left: -5px;
    border-width: 5px;
    border-style: solid;
    border-color: #555 transparent transparent transparent;
}
</style>