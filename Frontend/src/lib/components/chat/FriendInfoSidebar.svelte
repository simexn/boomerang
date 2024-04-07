<script lang="ts">
    import type { User } from "$lib/Handlers/accountHandler";
    import type { Group } from "$lib/Handlers/groupHandler";
    import { fly, slide } from "svelte/transition";
    import "$lib/css/sidebarstyles.css"
    import { userStatuses } from "$lib/stores/userStatusesStore";
    import { handleRemoveFriend, type FriendInfo } from "$lib/Handlers/userHandler";
    import { goto } from "$app/navigation";
    import { onMount } from "svelte";

    export let groupInfo: Group;
    export let userInfo: User;
    export let friendInfo: FriendInfo;
    export let imageUrl: string = '/user-icon-placeholder.png';
    export let chatId: string;
    export let isInfoSidebarOpen: boolean;
            let mutualServersDropdown: boolean = false;
            let mutualFriendsDropdown: boolean = false;
            let userOptionsDropdown: boolean = false;

    let isUserHovered = false;

    onMount(() => {
        console.log("friend pfp" + friendInfo.userPfp)
    });

    async function removeFriend(){
        await handleRemoveFriend(friendInfo.id);
        await goto('/chat/home');
    }
    function openDropdown(userId : any, event: any) {
        event.stopPropagation();
    }
</script>
<div class="sidebar-info" transition:fly="{{x: 1000, duration: 500}}">
    <div class="sidebar-header">
        <span class="sidebar-title">
            <span style="line-height: 2.4rem;font-family: Metropolis, sans-serif !important;">
                Info
            </span>
            <span class="sidebar-subtitle">{friendInfo?.username}</span>
        </span><button class="sidebar-close-btn" aria-label="Close" on:click={() =>isInfoSidebarOpen = false}>
            <i class="fa-solid fa-x"></i>
        </button>
    </div>

    <div class="user-info-section">
        <!-- svelte-ignore a11y-no-static-element-interactions -->
        <!-- svelte-ignore a11y-click-events-have-key-events -->
        <div class="user-info-options" on:click={() => userOptionsDropdown = !userOptionsDropdown}>
            <i class="fas fa-ellipsis-v"></i>
            {#if userOptionsDropdown}
                <div role="navigation" class="dropdown-menu show">
                    <button class="dropdown-item" on:click={() => removeFriend()}>Unfriend</button>
                    <button class="dropdown-item" on:click|stopPropagation={null}>Block</button>
                </div>
            {/if}
        </div>
        <div class="user-info-image-container">
            <div class="user-info-image">
                <img src={friendInfo.userPfp} style="border-radius:50%;" width="50px" height="50px" alt="user image" />
                <span class="user-status-dot" class:online={$userStatuses[friendInfo?.id.toString()] == 'online'}></span>
            </div>
        </div>
        <div class="user-info-details">
            <p class="user-info-username">{friendInfo?.username}</p>
        </div>
        <hr class="sidebar-info-separator"/>
        <div class="user-info-since">
            <h5>Boomerang member since:</h5>
            {friendInfo?.memberSince}
        </div>
        <hr class="sidebar-info-separator"/>
        <div class="user-info-since">
            <h5>Friends since:</h5>
            {friendInfo?.friendsSince}
        </div>
        
    </div>

    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <div class="user-info-section-mutual">
       
        <!-- svelte-ignore a11y-no-static-element-interactions -->
        <div class="user-info-servers" on:click={()=>mutualServersDropdown = !mutualServersDropdown}>
            <div class="user-info-mutual-button">
            <p>Mutual servers </p> <!-- replace with actual data -->
            <i class="fas fa-chevron-right" class:rotate={mutualServersDropdown}></i>
            </div>
            {#if mutualServersDropdown}
            <div class="dropdown-container" class:active={mutualFriendsDropdown} transition:slide={{duration: 500}}>
                {#if friendInfo?.mutualGroups?.length == 0}
                <div class="dropdown-container" style="justify-content:center; display:flex;">              
                    <p style="font-size: 0.9rem; margin-top: 0.9rem;"><i>No mutual servers</i></p>                         
                </div>
                {:else}
                <div class="dropdown-container" class:active={mutualFriendsDropdown} transition:slide={{duration: 500}}> 
                    <ul class="nav nav-pills flex-column mb-auto">
                            
                        {#each friendInfo?.mutualGroups || [] as group} 
                        <li class="nav-item" style="" transition:slide={{duration: 300}}>
                            <a class="nav-link sidebar-group" href={`/chat/${group?.id}`} style="">
                                <div style="display:inline-block; position:relative;">
                                    <span class="user-status-dot" class:online={$userStatuses[group?.id.toString()] == 'online'}></span>
                                </div>
                                <b>{group?.name}</b>
                            </a>
                        </li>
                        {/each}
                    </ul>
                </div>
                {/if}
            </div>
            {/if}
            
        </div>
        <!-- svelte-ignore a11y-click-events-have-key-events -->
        <!-- svelte-ignore a11y-no-static-element-interactions -->
        <div class="user-info-friends" on:click={()=>mutualFriendsDropdown = !mutualFriendsDropdown}>
            <div class="user-info-mutual-button">
            <p>Mutual friends </p>
            <i class="fas fa-chevron-right" class:rotate={mutualFriendsDropdown}></i>
            </div>
            {#if mutualFriendsDropdown}
            <div class="dropdown-container" class:active={mutualFriendsDropdown} transition:slide={{duration: 500}}>
                {#if friendInfo?.mutualFriends?.length == 0}
                <div class="dropdown-container" style="justify-content:center; display:flex;"> 
                    
                    
                    <p style="font-size: 0.9rem; margin-top: 0.9rem;"><i>No mutual friends</i></p>
                    
                    
                </div>
                {:else}
                
                    <ul class="nav nav-pills flex-column mb-auto">

                        {#each friendInfo?.mutualFriends || [] as friend} 
                        <li class="nav-item" style="" transition:slide={{duration: 300}}>
                            <a class="nav-link sidebar-group" href={`/chat/me/${friend?.chatId}`} style="">
                                <div style="display:inline-block; position:relative;">
                                    <img width="40px" height="40px" style="border-radius: 50%;" src="{friend.userPfp}">
                                    <span class="user-status-dot" class:online={$userStatuses[friend?.id.toString()] == 'online'}></span>
                                </div>
                                <b>{friend?.username}</b>
                            </a>
                        </li>
                        {/each}
                    </ul>
                
                {/if}
            </div>
            {/if}
        </div>
    </div>
</div>

<style>
    .dropdown-menu{
                position: absolute;
                top: 100%; /* This positions the dropdown right under the user div */
                right: -2rem;
            }
        
        .dropdown-item:hover{
            background-color: red;
            color:white;
            transition: background-color 0.3s;
        }
    .user-info-section-mutual{
        overflow-wrap: anywhere;
        
        font-size: 14px;
        line-height: 20px;
        border:solid 1px;
        border-radius: 10px;
        margin: 32px 24px 12px;
        position: relative;
        
    }
    .user-info-mutual-button{
        display: flex;
        flex-direction: row;
        align-items: center;
        width:100%;
        justify-content: space-between;
    }
        .user-info-section-mutual p{
            margin: 0px;
            font-size: 1rem;
            font-weight: 600;
        }
        .user-info-servers, .user-info-friends{
            width: 100%;
            padding: 0.75rem 1rem;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            
        }
        .user-info-friends{
            
            border-top: 1px solid rgb(var(--bg-sec))
        }
        .user-info-servers i, .user-info-friends i {
            font-size: 12px; /* adjust as needed */
        }
    .sidebar-info-separator{
        margin-top: 0.25rem;
    }
    .user-info-options{
        position: absolute;
        top: 0.6rem; /* adjust as needed */
        right: 0.6rem; /* adjust as needed */
        font-size: 20px;
        border-radius: 50%;
        padding: 0.25rem 0.75rem 0.25rem 0.75rem;
    }
        .user-info-options:hover{
            background-color: var(--hover-dark);
        }
.user-status-dot {
    height: 13px;
    width: 13px;
    background-color: #bbb;
    border-radius: 50%;
    border: 1px solid black;
    display: inline-block;
    position: absolute;
    bottom: 2px;
    right: 2px;
    }
    .user-status-dot.online {
    background-color: #4CAF50;
    }
    
    
.sidebar-info {
    display: flex;
    flex-direction: column;
    position: fixed;
    right: 0;
    width: 26rem; /* adjust as needed */
    height: 100%;
    background-color: var(--sec-fg); /* adjust as needed */
    overflow-y: auto;
    box-sizing: border-box;
    transition: transform 0.3s ease-in-out;
    transform: translateX(0);
    flex-shrink: 0;
    border-left: solid 1px #e5e5e5;
    z-index:500;
    display: flex;
    flex: 1 1 auto;

}
    .user-info-section{
        overflow-wrap: anywhere;
        padding: 12px 24px 12px;
        font-size: 14px;
        line-height: 20px;
        border:solid 1px;
        border-radius: 10px;
        margin: 32px 24px 12px;
        position: relative;
        
    }
    .user-info-image-container {
    display: flex;
    position: absolute;
    align-items: center;
    margin-bottom: 12px;
    top: -1.75rem;
    border-radius: 50%;
    background-color: white;
}

.user-info-details {
    margin-left: 12px;
    margin-top: 0.25rem;
}

.user-info-since{
    margin-left: 12px;
    font-size: 14px; /* adjust as needed */
}
            .user-info-username{
                font-family: Metropolis, sans-serif;
                font-size: 18px;
                line-height: 24px;
                color: rgb(var(--bg-sec));
                font-weight: 600;
                margin: 0px;
            }   

            .nav-item :hover{
                background-color: var(--hover-dark);
            }
.rotate{
    transform: rotate(90deg);

}
.fa-chevron-right{
            transition: transform 0.3s cubic-bezier(0.68, -0.55, 0.27, 1.55);
        }

        @media only screen and (max-width: 600px) {
            .sidebar-info {
                width: 100%;
            }
        }
</style>
